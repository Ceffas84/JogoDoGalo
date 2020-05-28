using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    public class TSCryptography
    {
        private const int SALTSIZE = 8;
        private const int NUMBER_OF_ITERATIONS = 1000;
        private const int SECRET_SIZE = 8;
        private string SECRET = GenerateRandomSecretString(SECRET_SIZE);
        private AesCryptoServiceProvider Aes;
        private RSACryptoServiceProvider Rsa;
        //
        //Summary: 
        //      Construtor da classe TSCryptography
        //     Cria o objeto Aes com base num vetor de inicialização de numa chave simetrica aleatórias
        public TSCryptography()
        {
            Aes = new AesCryptoServiceProvider();
            Aes.IV = GenerateIV(SECRET);
            Aes.Key = GenerateSymKey(SECRET);
            Rsa = new RSACryptoServiceProvider();
        }
        //
        //Summary: 
        //      Construtor da classe TSCryptography
        //      Recebe como parametros o vetor de inicialização e a chave symétrica
        //      Atualiza o objeto Aes com esses parametros
        public TSCryptography(byte[] iv, byte[] key)
        {
            Aes = new AesCryptoServiceProvider();
            Aes.IV = iv;
            Aes.Key = key;
            Rsa = new RSACryptoServiceProvider();
        }

        private static string GenerateRandomSecretString(int size)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(90 * random.NextDouble() + 33)));
                if(ch == '"' || ch == '\\' || ch == '`' || ch == '.' || ch == ',' || ch == '´' || ch == '\'')
                {
                    i = i - 1;
                    continue;
                }
                builder.Append(ch);
            }
            return builder.ToString();
        }

        //
        //Summary:
        //      Função que gera um Vetor de Inicialização para utilizar no objeto AES
        //      Recebe como parametro um segredo
        //Returns:
        //      Um vetor de inicialização aleatório gerado atraves de um salt e de um numero de iterações
        private byte[] GenerateIV(string secret)
        {
            byte[] salt = GenerateSalt();
            Rfc2898DeriveBytes pwdGen = new Rfc2898DeriveBytes(secret, salt, NUMBER_OF_ITERATIONS);
            byte[] iv = pwdGen.GetBytes(16);
            return iv;
        }
        //
        //Summary:
        //      Função que gera uma Chave Simétrica para utilizar no objeto AES
        //      Recebe como parametro um segredo
        //Returns:
        //      Uma symetric key aleatória gerada atraves de um salt e de um numero de iterações
        private byte[] GenerateSymKey(string secret)
        {
            byte[] salt = GenerateSalt();
            Rfc2898DeriveBytes pwdGen = new Rfc2898DeriveBytes(secret, salt, NUMBER_OF_ITERATIONS);
            byte[] symKey = pwdGen.GetBytes(16);
            return symKey;
        }
        //
        //Summary:
        //      Função que faz a dencriptação simétrica de um dado array de bytes
        //      Recebe como parametro um array de bytes
        //Returns:
        //      Retorna um array de bytes encriptado com chave simétrica
        public byte[] SymetricEncryption(byte[] data)
        {
            byte[] encryptedData;

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, Aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(data, 0, data.Length);
                    //cs.Close();
                }
                //guardar os dados cifrados que estão em memória
                encryptedData = ms.ToArray();
                //ms.Close();
            }
            return encryptedData;
        }
        //
        //Summary:
        //      Função que faz a desencriptação simétrica de um dado array de bytes
        //      Recebe como parametro um array de bytes encryptado com chave simétrica
        //Returns:
        //      Retorna um array de bytes desencriptado
        public byte[] SymetricDecryption(byte[] encryptedData)
        {
            byte[] decryptedData;

            MemoryStream ms = new MemoryStream(encryptedData);

            CryptoStream cs = new CryptoStream(ms, Aes.CreateDecryptor(), CryptoStreamMode.Read);

            decryptedData = new byte[ms.Length];

            int bytesread = cs.Read(decryptedData, 0, decryptedData.Length);

            cs.Close();
            ms.Close();

            byte[] truncArray = new byte[bytesread];
            Array.Copy(decryptedData, truncArray, truncArray.Length);

            return truncArray;
        }
        //
        //Summary:
        //      Setter para a Propriedade SymKey
        public void SetAesSymKey(byte[] key)
        {
            Aes.Key = key;
        }
        //
        //Summary:
        //      Setter para a Propriedade IV
        public void SetAesIV(byte[] iv)
        {
            Aes.IV = iv;
        }
        //
        //Summary:
        //      Getter para a Propriedade SymKey
        public byte[] GetSymKey()
        {
            return Aes.Key;
        }
        //
        //Summary:
        //      Getter para a Propriedade IV
        public byte[] GetIV()
        {
            return Aes.IV;
        }
        //
        //Summary: 
        //      Função que faz a encriptação simétrica de um dado array de bytes
        //      Recebe como parametros um array de bytes e a chave a colocar na encriptação assimétrica (Publica ou Privada)
        //Returns: 
        //      Retorna um array de bytes encriptado pelo objeto Rsa e a chave recebido
        public byte[] RsaEncryption(byte[] data, string key)
        {
            Rsa.FromXmlString(key);
            byte[] encryptedData = Rsa.Encrypt(data, true);
            return encryptedData;
        }
        //
        //Summary: 
        //      Função que faz a desencriptação simétrica de um dado array de bytes
        //      Recebe como parametros um array de bytes e a chave a colocar na desencriptação assimétrica
        //Returns: 
        //      Retorna um array de bytes desencriptado pelo objeto Rsa e a chave recebida
        public byte[] RsaDecryption(byte[] data, string key)
        {
            Rsa.FromXmlString(key);
            byte[] decryptedData = Rsa.Decrypt(data, true);
            return decryptedData;
        }
        //
        //Summary:
        //      Retorna a Publick Key do Objecto RSA
        public string GetPublicKey()
        {
            return Rsa.ToXmlString(false);
        }
        //
        //Summary:
        //      Retorna a Publick e Private Key do Objecto RSA
        public string GetPrivateKey()
        {
            return Rsa.ToXmlString(true);
        }
        //
        //Summary:
        //      Retorna a Publick e Private Key do Objecto RSA
        public void SetRsaPublicKeyCryptography(string publicKey)
        {
            Rsa.FromXmlString(publicKey);
        }

        public void SetRsaPrivateKeyCryptography(string privateKey)
        {
            Rsa.FromXmlString(privateKey);
        }
        //
        //Summary: 
        //      Função que gera um salt aleatório de 8 bytes
        //Returns: 
        //      Retorna um array de bytes com o salt aleatório gerado
        public byte[] GenerateSalt()
        {
            //Generate a cryptographic random number.
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] buff = new byte[SALTSIZE];
            rng.GetBytes(buff);
            return buff;
        }
        //
        //Summary: 
        //      Função que gera uma salted hash de 32
        //      Recebe como parametros uma string e um salt
        //Returns: 
        //      Retorna uma saltedhash da string introduzida
        public byte[] GenerateSaltedHash(string plainText, byte[] salt)
        {
            Rfc2898DeriveBytes rfc2898 = new Rfc2898DeriveBytes(plainText, salt, NUMBER_OF_ITERATIONS);
            return rfc2898.GetBytes(32);
        }
        public static byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
        public static Object ByteArrayToObject(byte[] byteArray)
        {
            MemoryStream memoryStream = new MemoryStream();
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            memoryStream.Write(byteArray, 0, byteArray.Length);
            memoryStream.Seek(0, SeekOrigin.Begin);
            Object obj = (Object)binaryFormatter.Deserialize(memoryStream);
            return obj;
        }
        public static byte[] GenerateHash(byte[] data)
        {
            byte[] hash;
            using (SHA1 sha1 = SHA1.Create())
            {
                hash = sha1.ComputeHash(data);
            }
            return hash;
        }
        public byte[] SignHash(byte[] hash, string key)
        {
            Rsa.FromXmlString(key);
            byte[] signature;
            using (SHA1 sha1 = SHA1.Create())
            {
                signature = Rsa.SignHash(hash, CryptoConfig.MapNameToOID("SHA1"));

            }
            return signature;
        }
        public byte[] SignData(byte[] data, string key)
        {
            Rsa.FromXmlString(key);

            byte[] signedData;
            using (SHA1 sha1 = SHA1.Create())
            {
                signedData = Rsa.SignData(data, sha1);
            }
            return signedData;
        }
        public bool VerifyHash(byte[] hash, byte[] signature, string key)
        {
            //SetRsaPublicKeyCryptography(key);
            return Rsa.VerifyHash(hash, CryptoConfig.MapNameToOID("SHA1"), signature);
        }
        public bool VerifyData(byte[] data, byte[] signature, string key)
        {
            SetRsaPublicKeyCryptography(key);
            bool verify;
            string a = Encoding.UTF8.GetString(data);
            using (SHA1 sha1 = SHA1.Create())
            {
                verify = Rsa.VerifyData(data, sha1, signature);
            }
            return verify;
        }
    }
}
