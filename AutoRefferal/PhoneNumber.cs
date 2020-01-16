using System.IO;
using System.Net;

namespace AutoRefferal
{
    /// <summary>
    /// Класс описывающий телефон
    /// </summary>
    public class PhoneNumber
    {
        /// <summary>
        /// Ключ апи
        /// </summary>
        public string ApiKey { get; set; }
        /// <summary>
        /// Ид
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Номер
        /// </summary>
        public string Number { get; set; }
        /// <summary>
        /// Статус код
        /// </summary>
        public string StatusCode { get; set; }
        /// <summary>
        /// Код смс
        /// </summary>
        public string Code { get; set; }

        public bool UseAgain { get; set; }

        public PhoneNumber()
        {

        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="statusCode">Статус код</param>
        public PhoneNumber(string statusCode)
        {
            StatusCode = statusCode;
        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="statusCode">Статус код</param>
        /// <param name="id">Ид</param>
        /// <param name="number">Номер телефона</param>
        public PhoneNumber(string statusCode, string id, string number)
        {
            Id = id;
            var c = number.Remove(0, 1);
            Number = number.Remove(0, 1);
            StatusCode = statusCode;
        }

        /// <summary>
        /// Установка ключа апи
        /// </summary>
        /// <param name="key">Ключ апи</param>
        public void SetApiKey(string key)
        {
            ApiKey = key;
        }

        /// <summary>
        /// Получение телефона для отправки смс
        /// </summary>
        public void GetPhoneNumber()
        {
            WebRequest request = WebRequest.Create("http://sms-activate.ru/stubs/handler_api.php?api_key=" + ApiKey + "&action=getNumber&service=fx&operator=any&country=0");//get number
            WebResponse response = request.GetResponse();
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    var result = reader.ReadToEnd();
                    if (result.Contains("ACCESS_NUMBER"))
                    {
                        var num = result.Split(':');
                        StatusCode = num[0];
                        Id = num[1];
                        Number = num[2];
                    }
                    else
                    {
                        StatusCode = result;
                    }
                }
            }
        }

        /// <summary>
        /// Отправка уведомления об отправленном смс
        /// </summary>
        public void MessageSend()
        {
            WebRequest request = WebRequest.Create("http://sms-activate.ru/stubs/handler_api.php?api_key=" + ApiKey + "&action=setStatus&status=1&id=" + Id);//activate number
            WebResponse response = request.GetResponse();
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    var result = reader.ReadToEnd();
                    StatusCode = result;
                }
            }
        }

        /// <summary>
        /// Получаем смс код
        /// </summary>
        public void GetCode()
        {
            WebRequest request = WebRequest.Create("http://sms-activate.ru/stubs/handler_api.php?api_key=" + ApiKey + "&action=getStatus&id=" + Id);//get message
            WebResponse response = request.GetResponse();
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    var result = reader.ReadToEnd();
                    if (result.Contains("STATUS_OK"))
                    {
                        var res = result.Split(':');
                        StatusCode = res[0];
                        Code = res[1];
                    }
                    else
                    {
                        StatusCode = result;
                    }
                }
            }
        }

        public bool RetryCode()
        {
            WebRequest request = WebRequest.Create("http://sms-activate.ru/stubs/handler_api.php?api_key=" + ApiKey + "&action=setStatus&status=3&id=" + Id);//activate number
            WebResponse response = request.GetResponse();
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {

                    var result = reader.ReadToEnd();
                    StatusCode = result;
                    if (result.Contains("ACCESS_RETRY_GET"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// Сообщаем об успешном использовании номера
        /// </summary>
        public void NumberConformation()
        {
            WebRequest request = WebRequest.Create("http://sms-activate.ru/stubs/handler_api.php?api_key=" + ApiKey + "&action=setStatus&status=6&id=" + Id);//activate number
            WebResponse response = request.GetResponse();
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    var result = reader.ReadToEnd();
                    StatusCode = result;
                }
            }
        }

        /// <summary>
        /// Сообщаем об отмене использования номера
        /// </summary>
        public void DeclinePhone()
        {
            WebRequest request = WebRequest.Create("http://sms-activate.ru/stubs/handler_api.php?api_key=" + ApiKey + "&action=setStatus&status=-1&id=" + Id);//activate number
            WebResponse response = request.GetResponse();
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    var result = reader.ReadToEnd();
                    StatusCode = result;
                }
            }
        }

    }
}
