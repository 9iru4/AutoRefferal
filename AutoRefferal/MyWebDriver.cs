using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Opera;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;

namespace AutoRefferal
{
    public enum RegistrationState { Confirmed, NotConfirmed, NotRegistered }

    public enum AutoRegState { StoppedByUser, NotEnoughProxies, NotEnoughAccounts, NotEnoughRefferals, StoppedByException, SMSServiceCrashed }
    /// <summary>
    /// Класс описывающий взаимодейстиве с браузером
    /// </summary>
    public class MyWebDriver
    {
        /// <summary>
        /// Веб драйвер
        /// </summary>
        IWebDriver driver { get; set; }
        /// <summary>
        /// Список аккаунтов
        /// </summary>
        public List<Account> accounts { get; set; }
        /// <summary>
        /// Телефон
        /// </summary>
        public PhoneNumber phone { get; set; }
        /// <summary>
        /// Список рефералов
        /// </summary>
        public List<Refferal> refferals { get; set; }
        /// <summary>
        /// Настройки
        /// </summary>
        public MySettings settings;
        /// <summary>
        /// Сипсок прокси
        /// </summary>
        public List<MyProxy> myProxies { get; set; }

        MyProxy currentProxy;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public MyWebDriver()
        {

        }

        /// <summary>
        /// Инициализация класса
        /// </summary>
        public void InitializeWebDriver()
        {
            accounts = Account.LoadAccounts();
            refferals = Refferal.LoadRefferals();
            myProxies = MyProxy.LoadProxies();
            settings = new MySettings();
            settings.LoadSettings();
            phone = new PhoneNumber();
            phone.SetApiKey(settings.SmsApiKey);
        }

        /// <summary>
        /// Установить ключ апи для смс сервиса
        /// </summary>
        public void SetApiKey()
        {
            phone.ApiKey = settings.SmsApiKey;
        }

        /// <summary>
        /// Использование оперы в качестве браузера
        /// </summary>
        public void InitializeOperaDriver()
        {
            OperaOptions options = new OperaOptions();
            options.BinaryLocation = settings.OperaPath;
            options.AddArgument("user-data-dir=" + Directory.GetCurrentDirectory() + @"\opera");
            options.AddArgument("private");
            driver = new OperaDriver(options);
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(40);
        }

        /// <summary>
        /// Использование хрома в качестве браузера
        /// </summary>
        public void InitializeChromeWithProxy()
        {
            ChromeOptions options = new ChromeOptions();
            foreach (var item in myProxies.ToList())
            {
                if (item.UsedActivation >= 3)
                    continue;
                else
                {
                    currentProxy = new MyProxy(item.IpAddress, item.Port);
                    options.AddArguments("--proxy-server=socks4://" + item.IpAddress + ":" + item.Port);
                    break;
                }
            }
            options.AddArgument("--incognito");
            options.AddArgument("--user-data-dir=" + Directory.GetCurrentDirectory() + @"\Chrome");
            driver = new ChromeDriver(options);
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(40);
        }

        /// <summary>
        /// Добавление новых аккаунтов из файла
        /// </summary>
        public void AddNewAccounts(string pathToFile)
        {
            var res = Account.AddNewAccounts(accounts, pathToFile);
            if (res == null)
            {
                MessageBox.Show("Аккаунты не добавлены.");
            }
            else
                accounts = res;
            Account.SaveAccounts(accounts);
        }

        /// <summary>
        /// Добавление новых рефеалов из файла
        /// </summary>
        public void AddNewRefferals(string pathToFile)
        {
            var res = Refferal.GetNewRefferals(refferals, pathToFile);
            if (res == null)
            {
                MessageBox.Show("Реферальные коды не добавлены.");
            }
            else
                refferals = res;
            Refferal.SaveRefferals(refferals);
        }

        /// <summary>
        /// Добавление новых прокси из файла
        /// </summary>
        public void AddNewProxies(string pathToFile)
        {
            var res = MyProxy.GetNewProxies(myProxies, pathToFile);
            if (res == null)
            {
                MessageBox.Show("Прокси не добавлены.");
            }
            else
                myProxies = res;
            MyProxy.SaveProxies(myProxies);
        }

        /// <summary>
        /// Остановка драйвера
        /// </summary>
        public void Quit()
        {
            try
            {
                driver.Quit();
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Проверка состояния регистрации
        /// </summary>
        /// <returns>Зарегистрирован, Зарегистрирован но не подтвержден, не зарегистрирован</returns>
        public RegistrationState CheckRegistrationState()
        {
            if (driver.FindElements(By.ClassName("confirmed")).Count == 2)
            {
                WriteLog(accounts[0].Email + ":" + "2");
                return RegistrationState.Confirmed;
            }
            if (driver.FindElements(By.ClassName("confirmed")).Count == 1)
            {
                WriteLog(accounts[0].Email + ":" + "1");
                return RegistrationState.NotConfirmed;
            }
            else
            {
                WriteLog(accounts[0].Email + ":" + "0");
                return RegistrationState.NotRegistered;
            }

        }

        /// <summary>
        /// Получение кода регистрации по смс
        /// </summary>
        /// <returns>Получен ли код</returns>
        public bool GetCode(bool isRetry)
        {
            foreach (var item in phone.Number)
            {
                driver.FindElement(By.Name("RegistrationForm[phone]")).SendKeys(item.ToString());
                Thread.Sleep(30);
            }
            Thread.Sleep(1000);
            try
            {
                driver.FindElement(By.Id("phone_code-button")).Click();
                Thread.Sleep(1000);
                driver.SwitchTo().Alert().Accept();
                Thread.Sleep(1000);
            }
            catch (Exception e)
            {
                WriteLog(e.ToString());
                return false;
            }

            try
            {
                if (isRetry)
                    phone.RetryCode();
                else
                    phone.MessageSend();
                Thread.Sleep(15000);
                phone.GetCode();
                if (phone.StatusCode.Contains("STATUS_OK"))
                {
                    foreach (var item in phone.Code)
                    {
                        driver.FindElement(By.Name("RegistrationForm[sms]")).SendKeys(item.ToString());
                        Thread.Sleep(30);
                    }
                    Thread.Sleep(1000);
                    return true;
                }
                else
                {
                    Thread.Sleep(15000);
                    phone.GetCode();
                    if (phone.StatusCode.Contains("STATUS_OK"))
                    {
                        foreach (var item in phone.Code)
                        {
                            driver.FindElement(By.Name("RegistrationForm[sms]")).SendKeys(item.ToString());
                            Thread.Sleep(30);
                        }
                        Thread.Sleep(1000);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex.ToString());
                phone.GetCode();
                if (phone.StatusCode.Contains("STATUS_OK"))
                {
                    driver.FindElement(By.Name("RegistrationForm[sms]")).SendKeys(phone.Code.ToString());
                    return true;
                }
                else return false;
            }
        }

        /// <summary>
        /// Получение номера телефона для регистрации
        /// </summary>
        /// <returns>Получен ли номер</returns>
        public bool GetNumberPhone()
        {
            phone.GetPhoneNumber();
            if (phone.StatusCode.Contains("ACCESS_NUMBER"))
            {
                return true;
            }
            else
            {
                WriteLog(phone.StatusCode);
                return false;
            }
        }

        /// <summary>
        /// Сообщаем об отмене использования номера
        /// </summary>
        /// <param name="number">Телефон</param>
        public void DeclinePhone()
        {
            if (phone.Number != null)
            {
                phone.DeclinePhone();
            }
        }

        /// <summary>
        /// Возможна ли регистрация в настоящее время
        /// </summary>
        /// <returns>данет</returns>
        public bool IsRegistrationAvaliable()
        {
            if (driver.FindElement(By.ClassName("field-registrationform-first_name")).GetAttribute("innerHTML").Contains("в настоящее время регистрация невозможна"))
            {
                myProxies.Where(x => x.IpAddress == currentProxy.IpAddress).FirstOrDefault().UsedActivation = 3;
                MyProxy.SaveProxies(myProxies);
                return false;
            }
            else return true;
        }

        /// <summary>
        /// Использован ли имейл
        /// </summary>
        /// <param name="account">Аккаунт на котором проводилась регистрация</param>
        /// <returns></returns>
        public bool IsEmailUsed(Account account)
        {
            if (driver.FindElement(By.ClassName("field-registrationform-email")).GetAttribute("innerHTML").Contains("зарегистрирова"))
            {
                account.SaveAccountInfo("Проблема с имейлом");
                accounts.Remove(account);
                Account.SaveAccounts(accounts);
                return false;
            }
            else return true;
        }

        /// <summary>
        /// Работает ли прокси
        /// </summary>
        /// <returns>данет</returns>
        public bool IsProxyCanUsed()
        {
            try
            {
                if (driver.PageSource.Contains("error-code"))
                {
                    myProxies.Where(x => x.IpAddress == currentProxy.IpAddress).FirstOrDefault().UsedActivation = 3;
                    MyProxy.SaveProxies(myProxies);
                    return false;
                }
                else return true;
            }
            catch (Exception e)
            {
                WriteLog(e.ToString());
                return false;
            }

        }

        /// <summary>
        /// Регистрация подтверждена
        /// </summary>
        /// <param name="account">Аккаунт на котором проводилась регистрация</param>
        /// <param name="refferal">Реферальный код на котором проводилась регистация</param>
        public void RegisterConfirmed(Account account, Refferal refferal)
        {
            account.SaveAccountInfo("Зарегистрирован");
            accounts.Remove(accounts.First());
            Account.SaveAccounts(accounts);
            refferal.ActivatedAccounts++;
            Refferal.SaveRefferals(refferals);
            if (settings.SelectedBrowser == "Chrome")
            {
                var prx = myProxies.Where(x => x.IpAddress == currentProxy.IpAddress).FirstOrDefault();
                prx.UsedActivation++;
                myProxies.Remove(myProxies.Where(x => x.IpAddress == prx.IpAddress).FirstOrDefault());
                myProxies.Add(prx);
                MyProxy.SaveProxies(myProxies);
            }
        }

        /// <summary>
        /// Регистрация завершена, но не подтверждена
        /// </summary>
        /// <param name="account">Аккаунт на котором проводилась регистрация</param>
        public void RegistrationNotConfirmed(Account account)
        {
            account.SaveAccountInfo("Использован, но не подтвержден");
            accounts.Remove(accounts.First());
            Account.SaveAccounts(accounts);
        }

        /// <summary>
        /// Регистрация не завершена
        /// </summary>
        /// <param name="account">Аккаунт на котором проводилась регистрация</param>
        public void RegistrationNotComplited(Account account)
        {
            account.SaveAccountInfo("Аккаунт не зарегистирован");
            accounts.Remove(accounts.First());
            Account.SaveAccounts(accounts);
        }

        /// <summary>
        /// Начало работы атоматической регистрации
        /// </summary>
        /// <returns>Результат работы метода.</returns>
        public AutoRegState StartAutoReg(CancellationToken token)
        {
            try
            {
                for (; ; )
                {
                    try
                    {
                        if (token.IsCancellationRequested)
                            return AutoRegState.StoppedByUser;

                        if (refferals.Where(x => x.ActivatedAccounts < 10).Count() == 0)
                            return AutoRegState.NotEnoughRefferals;

                        if (accounts.Count == 0)
                            return AutoRegState.NotEnoughAccounts;

                        Random rnd = new Random();
                        var item = refferals[rnd.Next(0, refferals.Count)];

                        if (item.ActivatedAccounts == 10) continue;

                        if (settings.SelectedBrowser == "Chrome")
                        {
                            if (myProxies.Where(x => x.UsedActivation < 3).Count() == 0)
                                return AutoRegState.NotEnoughProxies;
                            InitializeChromeWithProxy();
                        }
                        else InitializeOperaDriver();

                        try
                        {
                            driver.Navigate().GoToUrl("https://pgbonus.ru/register");
                        }
                        catch (Exception ex)
                        {
                            WriteLog(ex.ToString());
                            Thread.Sleep(1000);
                        }

                        if (settings.SelectedBrowser == "Chrome" && !IsProxyCanUsed())
                        {
                            Quit();
                            continue;
                        }

                        SendName(accounts.First().Name);

                        SendEmail(accounts.First().Email);

                        SendRefferal(item.Code);

                        if (IsRegistrationAvaliable())
                        {
                            if (IsEmailUsed(accounts.First()))
                            {
                                CheckPass();

                                if (!GetNumberPhone())
                                {
                                    Quit();
                                    continue;
                                }

                                if (!GetCode(false))
                                {
                                    DeclinePhone();
                                    Quit();
                                    continue;
                                }

                                CheckAgrrement();

                                if (!IsRegistrationAvaliable())
                                {
                                    DeclinePhone();
                                    Quit();
                                    continue;
                                }

                                SubmitReg();

                                try
                                {
                                    driver.Navigate().GoToUrl("https://pgbonus.ru/lk#");
                                    Thread.Sleep(3000);
                                }
                                catch (Exception ex)
                                {
                                    WriteLog(ex.ToString());
                                }

                                switch (CheckRegistrationState())
                                {
                                    case RegistrationState.Confirmed:
                                        RegisterConfirmed(accounts.First(), item);
                                        phone.NumberConformation();
                                        break;
                                    case RegistrationState.NotConfirmed:
                                        RegistrationNotConfirmed(accounts.First());
                                        phone.NumberConformation();
                                        break;
                                    case RegistrationState.NotRegistered:
                                        RegistrationNotComplited(accounts.First());
                                        break;
                                }
                                Thread.Sleep(2000);
                            }
                        }
                        Quit();
                    }
                    catch (NoSuchElementException e)
                    {
                        Quit();
                        DeclinePhone();
                        WriteLog(e.ToString());
                    }
                    catch (WebDriverException e)
                    {
                        Quit();
                        DeclinePhone();
                        WriteLog(e.ToString());
                    }
                    catch (WebException e)
                    {
                        Quit();
                        DeclinePhone();
                        WriteLog(e.ToString());
                        return AutoRegState.SMSServiceCrashed;
                    }
                }
            }
            catch (Exception ex)
            {
                Quit();
                WriteLog(ex.ToString());
                return AutoRegState.StoppedByException;
            }
        }

        /// <summary>
        /// Нажатие элемента подтвердить
        /// </summary>
        public void SubmitReg()
        {
            try
            {
                driver.FindElement(By.ClassName("submit")).Click();
                Thread.Sleep(2000);
            }
            catch (Exception e)
            {
                WriteLog(e.ToString());
            }
        }

        /// <summary>
        /// Ввод имени для поля имя
        /// </summary>
        /// <param name="Name">Имя для ввода</param>
        public void SendName(string Name)
        {
            foreach (var item in Name)
            {
                driver.FindElement(By.Name("RegistrationForm[first_name]")).SendKeys(item.ToString());
                Thread.Sleep(30);
            }
            Thread.Sleep(1000);

        }

        /// <summary>
        /// Ввод имейла для поля имейл
        /// </summary>
        /// <param name="Email">Имейл для ввода</param>
        public void SendEmail(string Email)
        {
            foreach (var item in Email)
            {
                driver.FindElement(By.Name("RegistrationForm[email]")).SendKeys(item.ToString());
                Thread.Sleep(30);
            }
            Thread.Sleep(1000);
        }

        /// <summary>
        /// Ввод рефферального кода для поля пригласивший
        /// </summary>
        /// <param name="refferal"></param>
        public void SendRefferal(string refferal)
        {
            foreach (var item in refferal)
            {
                driver.FindElement(By.Name("RegistrationForm[rcode]")).SendKeys(item.ToString());
                Thread.Sleep(30);
            }
            Thread.Sleep(1000);
        }

        /// <summary>
        /// Нажатие на чекбокс сгенерировать пароль
        /// </summary>
        public void CheckPass()
        {
            driver.FindElement(By.XPath("//*[@id=\"random_pass\"]")).Click();
            Thread.Sleep(1000);
        }

        /// <summary>
        /// Нажатие на чекбокс подтвердить регистрацию
        /// </summary>
        public void CheckAgrrement()
        {
            driver.FindElement(By.XPath("//*[@id=\"registrationform-aggr\"]")).Click();
            Thread.Sleep(1000);
        }

        /// <summary>
        /// Логиование данных в файл
        /// </summary>
        /// <param name="log">Текст для логирования</param>
        public void WriteLog(string log)
        {
            using (StreamWriter sw = new StreamWriter("LOG/log.txt", true))
            {
                sw.WriteLine(DateTime.Now.ToString() + "   " + log);
            }
        }
    }
}
