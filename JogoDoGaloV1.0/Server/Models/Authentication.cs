using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    public class Authentication
    {
        TSCryptography tsCrypto;
        private string FULLPATH = @"C:\Users\Simão Pedro\source\repos\JogoDoGalo\JogoDoGalo_Server\GaloDB.mdf";
        //private string FULLPATH = @"C:\Users\ricgl\source\repos\JogoDoGalo\JogoDoGalo_Server\GaloDB.mdf";
        public Authentication()
        {
            tsCrypto = new TSCryptography();
        }
        public bool VerifyLogin(string username, string password)
        {
            SqlConnection conn = null;
            try
            {
                // Configurar ligação à Base de Dados
                conn = new SqlConnection();

                conn.ConnectionString = String.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename='" + FULLPATH + "';Integrated Security=True");

                //conn.ConnectionString = String.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename='C:\Users\Simão Pedro\source\repos\JogoDoGalo\JogoDoGalo_Server\GaloDB.mdf';Integrated Security=True");

                // Abrir ligação à Base de Dados
                conn.Open();

                // Declaração do comando SQL
                String sql = "SELECT * FROM Users WHERE Username = @username";
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = sql;

                // Declaração dos parâmetros do comando SQL
                SqlParameter param = new SqlParameter("@username", username);

                // Introduzir valor ao parâmentro registado no comando SQL
                cmd.Parameters.Add(param);

                // Associar ligação à Base de Dados ao comando a ser executado
                cmd.Connection = conn;

                // Executar comando SQL
                SqlDataReader reader = cmd.ExecuteReader();


                if (!reader.HasRows)
                {
                    //throw new Exception("Error while trying to access an user");
                    return false;
                }

                // Ler resultado da pesquisa
                reader.Read();

                // Obter Hash (password + salt)
                byte[] saltedPasswordHashStored = (byte[])reader["SaltedPasswordHash"];

                // Obter salt
                byte[] saltStored = (byte[])reader["Salt"];

                conn.Close();

                byte[] hash = tsCrypto.GenerateSaltedHash(password, saltStored);

                return saltedPasswordHashStored.SequenceEqual(hash);
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
                return false;
            }
        }
        public void Register(string username, byte[] saltedPasswordHash, byte[] salt)
        {
            SqlConnection conn = null;
            try
            {
                // Configurar ligação à Base de Dados
                conn = new SqlConnection();

                conn.ConnectionString = String.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename='" + FULLPATH + "';Integrated Security=True");

                //conn.ConnectionString = String.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename='';Integrated Security=True");


                // Abrir ligação à Base de Dados
                conn.Open();

                // Declaração dos parâmetros do comando SQL
                //SqlParameter paramId = new SqlParameter("@Id", default);
                SqlParameter paramUsername = new SqlParameter("@Username", username);
                SqlParameter paramSaltedPasswordHash = new SqlParameter("@SaltedPasswordHash", saltedPasswordHash);
                SqlParameter paramSalt = new SqlParameter("@Salt", salt);

                // Declaração do comando SQL
                String sql = "INSERT INTO Users (Username, SaltedPasswordHash, Salt) VALUES (@Username,@SaltedPasswordHash,@Salt)";

                // Prepara comando SQL para ser executado na Base de Dados
                SqlCommand cmd = new SqlCommand(sql, conn);

                // Introduzir valores aos parâmentros registados no comando SQL
                //cmd.Parameters.Add(paramId);
                cmd.Parameters.Add(paramUsername);
                cmd.Parameters.Add(paramSaltedPasswordHash);
                cmd.Parameters.Add(paramSalt);

                // Executar comando SQL
                int lines = cmd.ExecuteNonQuery();

                // Fechar ligação
                conn.Close();
                if (lines == 0)
                {
                    // Se forem devolvidas 0 linhas alteradas então o INSERT não foi executado com sucesso
                    throw new Exception("Error while inserting an user");
                }
                Console.WriteLine("Registado com sucesso");
            }
            catch (Exception e)
            {
                throw new Exception("Error while inserting an user:" + e.Message);
            }
        }
    }
}
