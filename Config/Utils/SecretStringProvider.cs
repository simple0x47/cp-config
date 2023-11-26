using System.Security;

namespace Cuplan.Config.Utils;

public class SecretStringProvider
{
    public static SecureString GetSecureStringFromString(string theString)
    {
        SecureString secureString = new();

        foreach (char c in theString) secureString.AppendChar(c);

        secureString.MakeReadOnly();

        return secureString;
    }
}