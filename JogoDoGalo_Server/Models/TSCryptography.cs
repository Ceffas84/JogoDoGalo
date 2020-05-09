using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JogoDoGalo_Server.Models
{
    public class TSCryptography
    {
        private const int SALTSIZE = 8;
        private const int NUMBER_OF_ITERATIONS = 1000;
        private const string SECRET = "alma da %casa";
        private AesCryptoServiceProvider Aes;
        private RSACryptoServiceProvider Rsa;
        //
        //Summary: Construtor da classe TSCryptography
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
        }
        //
        //Summary:
        //      Função que gera um Vetor de Inicialização para utilizar no objeto AES
        //      Recebe como parametro um segredo
        //Returns:
        //      Um vetor de inicialização aleatório gerado atraves de um salt e de um numero de iterações
        private byte[] GenerateIV(string secret)
        {
            byte[] salt = GenerateSalt(SALTSIZE);
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
        private static byte[] GenerateSymKey(string secret)
        {
            byte[] salt = GenerateSalt(SALTSIZE);
            Rfc2898DeriveBytes pwdGen = new Rfc2898DeriveBytes(secret, salt, NUMBER_OF_ITERATIONS);
            byte[] symKey = pwdGen.GetBytes(16);
            return symKey;
        }
        //
        //Summary: 
        //      Função que faz a encriptação simétrica de um dado array de bytes
        //      Recebe como parametros um array de bytes e a chave a colocar na encriptação assimétrica
        //Returns: 
        //      Retorna um array de bytes encriptado pelo objeto Rsa e a chave recebido
        public byte[] RsaEncryption(byte[] data, string key)
        {
            RSACryptoServiceProvider Rsa = new RSACryptoServiceProvider();
            Rsa.FromXmlString(key);
            byte[] encryptedData = Rsa.Encrypt(data, true);
            return encryptedData;
        }
        //
        //Summary: 
        //      Função que faz a desencriptação simétrica de um dado array de bytes
        //      Recebe como parametros um array de bytes e a chave a colocar na desencriptação assimétrica
        //Returns: 
        //      Retorna um array de bytes desencriptado pelo objeto Rsa e a chave recebido
        public byte[] RsaDecryption(byte[] data, string key)
        {
            RSACryptoServiceProvider Rsa = new RSACryptoServiceProvider();
            Rsa.FromXmlString(key);
            byte[] decryptedData = Rsa.Decrypt(data, true);
            return decryptedData;
        }
        //Função que faz a dencriptação simétrica de um dado array de bytes
        //Parameters: Recebe um array de bytes
        //Returns: Retorna um array de bytes encriptado
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
        //Função que faz a desencriptação simétrica de um dado array de bytes
        //Parametes: recebe um  array de bytes
        //Returns: retorna um array de bytes encriptado
        public byte[] SymetricDecryption(byte[] encryptedData)
        {
            byte[] decryptedData;

            MemoryStream ms = new MemoryStream(encryptedData);

            CryptoStream cs = new CryptoStream(ms, Aes.CreateDecryptor(), CryptoStreamMode.Read);

            decryptedData = new byte[ms.Length];

            cs.Read(decryptedData, 0, decryptedData.Length);

            cs.Close();
            ms.Close();
            return decryptedData;
        }
        //
        //
        public void SetAesSymKey(byte[] key)
        {
            Aes.Key = key;
        }
        //
        //
        public void SetAesIV(byte[] iv)
        {
            Aes.IV = iv;
        }
        //Função que gera um salt aleatório
        private static byte[] GenerateSalt(int size)
        {
            //Generate a cryptographic random number.
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] buff = new byte[size];
            rng.GetBytes(buff);
            return buff;
        }
        //Função que gera a Hash de um test com um salt aleatório
        public static byte[] GenerateSaltedHash(string plainText, byte[] salt)
        {
            Rfc2898DeriveBytes rfc2898 = new Rfc2898DeriveBytes(plainText, salt, NUMBER_OF_ITERATIONS);
            return rfc2898.GetBytes(32);
        }
        public string GetPublicKey()
        {
            return Rsa.ToXmlString(false);
        }
    }
}
