using System.Text;

namespace Dynamic.Core.Authentication
{
    public class AuthConfigOptions
    {
        public byte[] seckey { get; set; } = Encoding.UTF8.GetBytes("this is a s,,||\\ample key ---sdfawdjkhkae\r\nudhcakufhad\\ //u7246r27843i14c34hry");
    }
}
