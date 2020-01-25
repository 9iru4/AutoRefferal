using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Opera;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;

namespace AutoRefferal
{
    public enum RegistrationState { Confirmed, NotConfirmed, NotRegistered }

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
        PhoneNumber phone { get; set; }
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
            if (phone != null)
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
                accounts.Remove(accounts.Where(x => x.Email == account.Email).FirstOrDefault());
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
                    Quit();
                    return true;
                }
                else return false;
            }
            catch (Exception)
            {
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
            accounts.Remove(accounts.Where(x => x.Email == account.Email).FirstOrDefault());
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
            accounts.Remove(accounts.Where(x => x.Email == account.Email).FirstOrDefault());
            Account.SaveAccounts(accounts);
        }

        /// <summary>
        /// Регистрация не завершена
        /// </summary>
        /// <param name="account">Аккаунт на котором проводилась регистрация</param>
        public void RegistrationNotComplited(Account account)
        {
            account.SaveAccountInfo("Аккаунт не зарегистирован");
            accounts.Remove(accounts.Where(x => x.Email == account.Email).FirstOrDefault());
            Account.SaveAccounts(accounts);
            if (!phone.UseAgain)
                phone.UseAgain = true;
        }

        /// <summary>
        /// Начало работы атоматической регистрации
        /// </summary>
        public void StartAutoReg(CancellationToken token)
        {
            try
            {
                foreach (var item in refferals)
                {
                    var buffAccounts = new List<Account>(accounts);

                    if (accounts.Count == 0)
                    {
                        MessageBox.Show("Аккаунты закончились.");
                        Quit();
                        break;
                    }

                    if (buffAccounts.Count > 0 && item.ActivatedAccounts < 10)
                    {
                        for (int i = 0; item.ActivatedAccounts < 10; i++)
                        {
                            if (accounts.Count == 0)
                                break;

                            if (settings.SelectedBrowser == "Chrome")
                                InitializeChromeWithProxy();
                            else InitializeOperaDriver();

                            if (token.IsCancellationRequested)
                            {
                                Quit();
                                MessageBox.Show("Программа остановлена по требованию пользователя.");
                                return;
                            }

                            try
                            {
                                driver.Navigate().GoToUrl("https://pgbonus.ru/register");
                            }
                            catch (Exception ex)
                            {
                                WriteLog(ex.ToString());
                                Thread.Sleep(1000);
                            }

                            if (IsProxyCanUsed() && settings.SelectedBrowser == "Chrome")
                            {
                                i--;
                                Quit();
                                continue;
                            }
                            try
                            {
                                SendName(buffAccounts[i].Name);
                            }
                            catch (Exception ex)
                            {
                                WriteLog(ex.ToString());
                                i--;
                                Quit();
                                continue;
                            }

                            try
                            {
                                SendEmail(buffAccounts[i].Email);
                            }
                            catch (Exception ex)
                            {
                                WriteLog(ex.ToString());
                                i--;
                                Quit();
                                continue;
                            }

                            try
                            {
                                SendRefferal(item.Code);
                            }
                            catch (Exception ex)
                            {
                                WriteLog(ex.ToString());
                                i--;
                                Quit();
                                continue;
                            }

                            if (IsRegistrationAvaliable())
                            {
                                if (IsEmailUsed(buffAccounts[i]))
                                {
                                    CheckPass();

                                    if (!phone.UseAgain)
                                    {
                                        GetNumberPhone();
                                    }

                                    if (!GetCode(phone.UseAgain))
                                    {
                                        DeclinePhone();
                                        Quit();
                                        i--;
                                        phone.UseAgain = false;
                                        continue;
                                    }

                                    CheckAgrrement();

                                    if (IsRegistrationAvaliable())
                                    {

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
                                                RegisterConfirmed(buffAccounts[i], item);
                                                phone.NumberConformation();
                                                break;
                                            case RegistrationState.NotConfirmed:
                                                RegistrationNotConfirmed(buffAccounts[i]);
                                                phone.NumberConformation();
                                                break;
                                            case RegistrationState.NotRegistered:
                                                RegistrationNotComplited(buffAccounts[i]);
                                                break;
                                        }
                                        Thread.Sleep(2000);
                                    }
                                    else
                                    {
                                        DeclinePhone();
                                        i--;
                                    }
                                }
                            }
                            Quit();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Quit();
                WriteLog(ex.ToString());
                MessageBox.Show("Произошло неведанное говно, программа остановленна.");
            }
        }

        /// <summary>
        /// Событие закрития окна
        /// </summary>
        private void Window_Closed(object sender, EventArgs e)
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
