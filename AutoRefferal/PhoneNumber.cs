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
    }
}
