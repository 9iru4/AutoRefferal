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
        List<Account> accounts { get; set; }
        /// <summary>
        /// Телефон
        /// </summary>
        PhoneNumber phone { get; set; }
        /// <summary>
        /// Список рефералов
        /// </summary>
        List<Refferal> refferals { get; set; }
        /// <summary>
        /// Настройки
        /// </summary>
        MySettings settings;
        /// <summary>
        /// Сипсок прокси
        /// </summary>
        List<MyProxy> myProxies { get; set; }

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
        }

        /// <summary>
        /// Использование хрома в качестве браузера
        /// </summary>
        public void InitializeChromeWithProxy()
        {
            ChromeOptions options = new ChromeOptions();
            foreach (var item in myProxies.ToList())
            {
                if (item.UsedActivation >= 10)
                    myProxies.Remove(item);
                else
                {
                    MyProxy.SaveProxies(myProxies);
                    break;
                }
            }
            options.AddArguments("--proxy-server=socks4://" + myProxies[0].IpAddress + ":" + myProxies[0].Port);
            options.AddArgument("private");
            driver = new ChromeDriver(options);
        }

        /// <summary>
        /// Добавление новых аккаунтов из файла
        /// </summary>
        public void AddNewAccounts()
        {
            var res = Account.GetNewAccounts(accounts);
            if (res == null)
                MessageBox.Show("Аккаунты не добавлены");
            else
                accounts = res;
        }

        /// <summary>
        /// Добавление новых рефеалов из файла
        /// </summary>
        public void AddNewRefferals()
        {
            var res = Refferal.GetNewRefferals(refferals);
            if (res == null)
                MessageBox.Show("Реферальные коды не добавлены");
            else
                refferals = res;
        }

        /// <summary>
        /// Добавление новых прокси из файла
        /// </summary>
        public void AddNewProxies()
        {
            var res = MyProxy.GetNewProxies(myProxies);
            if (res == null)
                MessageBox.Show("Прокси не добавлены");
            else
                myProxies = res;
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
        /// Получение кода регистрации по смс
        /// </summary>
        /// <returns>Получен ли код</returns>
        public bool GetCode()
        {
            foreach (var item in phone.Number)
            {
                driver.FindElement(By.Name("RegistrationForm[phone]")).SendKeys(item.ToString());
                Thread.Sleep(50);
            }
            Thread.Sleep(1000);
            driver.FindElement(By.Id("phone_code-button")).Click();
            Thread.Sleep(1000);
            try
            {
                driver.SwitchTo().Alert().Accept();
                phone.MessageSend();
                Thread.Sleep(15000);
                phone.GetCode();
                if (phone.StatusCode.Contains("STATUS_OK"))
                {
                    foreach (var item in phone.Code)
                    {
                        driver.FindElement(By.Name("RegistrationForm[sms]")).SendKeys(item.ToString());
                        Thread.Sleep(50);
                    }
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
                            Thread.Sleep(50);
                        }
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
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
        /// Начало работы атоматической регистрации
        /// </summary>
        public void StartAutoReg(CancellationToken token, bool useproxyformfile)
        {
            try
            {
                foreach (var item in refferals)
                {
                    var buffAccounts = new List<Account>(accounts);
                    if (accounts.Count == 0)
                    {
                        MessageBox.Show("Аккаунты закончились");
                    }
                    if (buffAccounts.Count > 0 && item.ActivatedAccounts < 10)
                    {
                        for (int i = 0; item.ActivatedAccounts < 10; i++)
                        {
                            if (useproxyformfile)
                                InitializeChromeWithProxy();
                            else InitializeOperaDriver();
                            if (token.IsCancellationRequested)
                            {
                                Quit();
                                MessageBox.Show("Программа остановлена");
                                return;
                            }
                            if (accounts.Count == 0)
                                break;
                            driver.Navigate().GoToUrl("https://pgbonus.ru/register");
                            Thread.Sleep(3000);
                            if (driver.PageSource.Contains("error-code"))
                            {
                                myProxies.RemoveAt(0);
                                MyProxy.SaveProxies(myProxies);
                                i--;
                                Quit();
                                continue;
                            }
                            SendName(buffAccounts[i].Name);
                            Thread.Sleep(3000);
                            SendEmail(buffAccounts[i].Email);
                            Thread.Sleep(3000);
                            SendRefferal(item.Code);
                            Thread.Sleep(4000);
                            if (!driver.FindElement(By.ClassName("field-registrationform-first_name")).GetAttribute("innerHTML").Contains("в настоящее время регистрация невозможна"))
                            {
                                if (!driver.FindElement(By.ClassName("field-registrationform-email")).GetAttribute("innerHTML").Contains("зарегистрирова"))
                                {
                                    CheckPass();
                                    Thread.Sleep(2000);
                                    if (GetNumberPhone())
                                    {
                                        if (GetCode())
                                        {
                                            Thread.Sleep(2000);
                                            CheckAgrrement();
                                            Thread.Sleep(3000);
                                            if (!driver.FindElement(By.ClassName("field-registrationform-first_name")).GetAttribute("innerHTML").Contains("в настоящее время регистрация невозможна"))
                                            {
                                                SubmitReg();
                                                phone.NumberConformation();
                                                Thread.Sleep(4000);
                                                driver.Navigate().GoToUrl("https://pgbonus.ru/lk#");
                                                Thread.Sleep(5000);
                                                if (driver.FindElements(By.ClassName("confirmed")).Count == 2)
                                                {
                                                    buffAccounts[i].SaveAccountInfo("Зарегистрирован");
                                                    accounts.Remove(accounts.Where(x => x.Name == buffAccounts[i].Name).FirstOrDefault());
                                                    Account.SaveAccounts(accounts);
                                                    item.ActivatedAccounts++;
                                                    Refferal.SaveRefferals(refferals);
                                                    myProxies[0].UsedActivation++;
                                                    var prox = myProxies[0];
                                                    myProxies.RemoveAt(0);
                                                    myProxies.Add(prox);
                                                    MyProxy.SaveProxies(myProxies);
                                                }
                                                else
                                                {
                                                    if (driver.FindElements(By.ClassName("confirmed")).Count == 1)
                                                    {
                                                        buffAccounts[i].SaveAccountInfo("Использован, но не подтвержден");
                                                        accounts.Remove(accounts.Where(x => x.Name == buffAccounts[i].Name).FirstOrDefault());
                                                        Account.SaveAccounts(accounts);
                                                    }
                                                    else
                                                    {
                                                        buffAccounts[i].SaveAccountInfo("Бабки просраны");
                                                        i--;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                DeclinePhone();
                                                i--;
                                            }

                                        }
                                        else
                                        {
                                            DeclinePhone();
                                            i--;
                                        }
                                    }
                                    else
                                    {
                                        i--;
                                    }
                                }
                                else
                                {
                                    DeclinePhone();
                                    accounts[i].SaveAccountInfo("Проблема с имейлом");
                                    accounts.Remove(accounts.Where(x => x.Name == buffAccounts[i].Name).FirstOrDefault());
                                    Account.SaveAccounts(accounts);
                                }
                            }
                            else
                            {
                                i--;
                            }
                            driver.Quit();
                            Thread.Sleep(1000);
                        }
                    }
                }
                MessageBox.Show("Рефферальные коды закончились");
            }
            catch (Exception ex)
            {

                DeclinePhone();
                WriteLog(ex.ToString());
                MessageBox.Show("Произошло неведанное говно, программа перешла в ручной режим.");
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
            driver.FindElement(By.ClassName("submit")).Click();
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
                Thread.Sleep(50);
            }

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
                Thread.Sleep(50);
            }
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
                Thread.Sleep(50);
            }
        }

        /// <summary>
        /// Нажатие на чекбокс сгенерировать пароль
        /// </summary>
        public void CheckPass()
        {
            driver.FindElement(By.XPath("//*[@id=\"random_pass\"]")).Click();
        }

        /// <summary>
        /// Нажатие на чекбокс подтвердить регистрацию
        /// </summary>
        public void CheckAgrrement()
        {
            driver.FindElement(By.XPath("//*[@id=\"registrationform-aggr\"]")).Click();
        }

        /// <summary>
        /// Логиование данных в файл
        /// </summary>
        /// <param name="log">Текст для логирования</param>
        public void WriteLog(string log)
        {
            using (StreamWriter sw = new StreamWriter("log.txt", true))
            {
                sw.WriteLine(DateTime.Now.ToString() + "   " + log);
            }
        }
    }
}
