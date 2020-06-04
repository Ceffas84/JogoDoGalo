using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    /**
     * <summary>    Class que implementa funções de manupilação dos
     *              dados de autenticação dos utilizadores do jogo  </summary>
     *
     * <remarks>    Ricardo Lopes, 04/06/2020. </remarks>
     */

    public class Authentication
    {
        TSCryptography tsCrypto;
        //private string FULLPATH = @"C:\Users\Simão Pedro\source\repos\JogoDoGalo\JogoDoGaloV1.0\Server\ClientsDB.mdf";
        private string FULLPATH = @"C:\USERS\RICGL\SOURCE\REPOS\JOGODOGALO\JOGODOGALOV1.0\SERVER\CLIENTSDB.MDF";

        /**
         * <summary>    Contrutor da Class Authentication que gera uma novo
         *              objeto de Cripto</summary>
         *
         * <remarks>    Ricardo Lopes, 04/06/2020. </remarks>
         */

        public Authentication()
        {
            tsCrypto = new TSCryptography();
        }

        /**
         * <summary>    Função que verifica se um username e uma password
         *              pertencem e validam um utilizador válido </summary>
         *
         * <remarks>    Ricardo Lopes, 04/06/2020. </remarks>
         *
         * <param name="username">  O username do utilizador </param>
         * <param name="password">  A password do utilizador </param>
         *
         * <returns>    Retorna verdadeiro se o utilizador foi autenticado, e false se não. </returns>
         */

        public bool VerifyLogin(string username, string password)
        {
            SqlConnection conn = null;
            try
            {
                // Configurar ligação à Base de Dados
                conn = new SqlConnection();
                conn.ConnectionString = String.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename='" + FULLPATH + "';Integrated Security=True");

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

                //Compara a saltedhash guardada na BD com a salted hash gerada através da password submetida 
                // e do salt que estava armazenado na BD
                return saltedPasswordHashStored.SequenceEqual(hash);
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
                return false;
            }
        }

        /**
         * <summary>    Funçaõ que faz o registo de um novo utilizador do jogo </summary>
         *
         * <remarks>    Ricardo Lopes, 04/06/2020. </remarks>
         *
         * <exception cref="Exception"> Lança uma exceção quando ocorre um erro de gravação na base de dados. </exception>
         *
         * <param name="username">              Recebe o username do utilizador. </param>
         * <param name="saltedPasswordHash">    Recebe a salted hash da password do utilizador. </param>
         * <param name="salt">                  Recebe o salt utilizado na formação da saltedhash. </param>
         */

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

        /**
         * <summary>    Função que devolve od Id de um utilizador através do
         *              username </summary>
         *
         * <remarks>    Ricardo Lopes, 04/06/2020. </remarks>
         *
         * <param name="username">  Recebe o username a localizar na BD. </param>
         *
         * <returns>    Se encontrar o utilizador devolve o seu Id, caso contrário devolve 0. </returns>
         */

        public int UserId(string username)
        {
            SqlConnection conn = null;
            try
            {
                // Configurar ligação à Base de Dados
                conn = new SqlConnection();
                conn.ConnectionString = String.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename='" + FULLPATH + "';Integrated Security=True");

                // Abrir ligação à Base de Dados
                conn.Open();

                // Declaração do comando SQL
                String sql = "SELECT Id FROM Users WHERE Username = @username";
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
                    return 0;
                }

                // Ler resultado da pesquisa
                reader.Read();

                // Obter o Id
                int id = (int)reader["Id"];

                conn.Close();

                return id;
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
                return 0;
            }
        }
    }
}
