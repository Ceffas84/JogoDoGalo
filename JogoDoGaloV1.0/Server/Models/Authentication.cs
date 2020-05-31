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
        //private string FULLPATH = @"C:\Users\Simão Pedro\source\repos\JogoDoGalo\JogoDoGaloV1.0\Server\ClientsDB.mdf";
        private string FULLPATH = @"C:\USERS\RICGL\SOURCE\REPOS\JOGODOGALO\JOGODOGALOV1.0\SERVER\CLIENTSDB.MDF";

        /**
         * <summary>    Construtor do objecto Authentication </summary>
         *
         * <remarks>    Ricardo Lopes, 31/05/2020. </remarks>
         */

        public Authentication()
        {
            tsCrypto = new TSCryptography();
        }

        /**
         * <summary>    Função de verifica se as credenciais de um determinado user são válida </summary>
         *
         * <remarks>    Ricardo Lopes, 31/05/2020. </remarks>
         *
         * <param name="username">  O username a verificar. </param>
         * <param name="password">  A password de acesso a verifica </param>
         *
         * <returns>    Retorna verdadeiro de as credenciais forem vális, falso se não forem válidas. </returns>
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

                //Fecha ligação à base de dados
                conn.Close();

                //Cria a slate hash da password enviada
                byte[] saltedPasswordHash = tsCrypto.GenerateSaltedHash(password, saltStored);

                //Retorna se a comparação da salted hash da password enviada é igual à salted hash da pasword armazenada
                return saltedPasswordHashStored.SequenceEqual(saltedPasswordHash);
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
                return false;
            }
        }

        /**
         * <summary>    Função que regista um novo utilizador na base de dados </summary>
         *
         * <remarks>    Ricardo Lopes, 31/05/2020. </remarks>
         *
         * <exception cref="Exception"> Retorna uma exceção se existir algum erro na gravação do utilizador. </exception>
         *
         * <param name="username">              O username do novo utilizador. </param>
         * <param name="saltedPasswordHash">    A salted hash da password (guarda-se na base de dados a hash de uma salted password, 
         *                                      para que não existam hash's iguais na base de dados - caso sejamutilizadas passwords iguais) </param>
         * <param name="salt">                  O salt da utilizado na . </param>
         */

        public void Register(string username, byte[] saltedPasswordHash, byte[] salt)
        {
            SqlConnection conn = null;
            try
            {
                // Configurar ligação à Base de Dados
                conn = new SqlConnection();
                conn.ConnectionString = String.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename='" + FULLPATH + "';Integrated Security=True");

                // Abrir ligação à Base de Dados
                conn.Open();

                // Declaração dos parâmetros do comando SQL
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
         * <summary>    Função que verifica e devolve se o id de um utilizador através do username </summary>
         *
         * <remarks>    Ricardo Lopes, 31/05/2020. </remarks>
         *
         * <param name="username">  O username a procurar. </param>
         *
         * <returns>    Devolve o id do user com o username enviado por parametro. </returns>
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
                    //Se a consulta não der resultados então devolve 0
                    return 0;
                }

                //Ler resultado da pesquisa
                reader.Read();

                // Obtém o Id 
                int id = (int)reader["Id"];

                //Fecha a ligação à base de dados
                conn.Close();

                //Retorna o id do user com o username enviado por parametro
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
