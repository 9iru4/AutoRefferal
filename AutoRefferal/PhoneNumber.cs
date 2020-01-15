using System.IO;
using System.Net;

namespace AutoRefferal
{
    public class PhoneNumber
    {
        public string Id { get; set; }
        public string Number { get; set; }

        public string StatusCode { get; set; }

        public int Code { get; set; }

        public PhoneNumber()
        {

        }

        public PhoneNumber(string statusCode)
        {
            StatusCode = statusCode;
        }

        public PhoneNumber(string statusCode, string id, string number)
        {
            Id = id;
            var c = number.Remove(0, 1);
            Number = number.Remove(0, 1);
            StatusCode = statusCode;
        }

        /// <summary>
        /// Получение телефона для отправки смс
        /// </summary>
        public void GetPhoneNumber()
        {
            WebRequest request = WebRequest.Create("http://sms-activate.ru/stubs/handler_api.php?api_key=1b2c6c99b521dAed531d16449226396d&action=getNumber&service=fx&operator=any&country=0");//get number
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
            WebRequest request = WebRequest.Create("http://sms-activate.ru/stubs/handler_api.php?api_key=1b2c6c99b521dAed531d16449226396d&action=setStatus&status=1&id=" + Id);//activate number
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
            WebRequest request = WebRequest.Create("http://sms-activate.ru/stubs/handler_api.php?api_key=1b2c6c99b521dAed531d16449226396d&action=getStatus&id=" + Id);//get message
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
                        Code = int.Parse(res[1]);
                    }
                    else
                    {
                        StatusCode = result;
                    }
                }
            }
        }

        /// <summary>
        /// Сообщаем об успешном использовании номера
        /// </summary>
        public void NumberConformation()
        {
            WebRequest request = WebRequest.Create("http://sms-activate.ru/stubs/handler_api.php?api_key=1b2c6c99b521dAed531d16449226396d&action=setStatus&status=6&id=" + Id);//activate number
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
            WebRequest request = WebRequest.Create("http://sms-activate.ru/stubs/handler_api.php?api_key=1b2c6c99b521dAed531d16449226396d&action=setStatus&status=-1&id=" + Id);//activate number
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
