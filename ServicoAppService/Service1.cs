using RestSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace ServicoAppService
{
    public partial class Service1 : ServiceBase
    {
        Timer timer;
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            timer = new Timer(new TimerCallback(Main), null, 0, (int)tempo.Hora);
        }

        protected override void OnStop()
        {
            EventLog.WriteEntry("Serviço Parado", EventLogEntryType.Warning);
        }

        protected async void Main(object sender)
        {
            DateTime hora = new DateTime(2022, 9, 1, 11, 0, 0);

            if (DateTime.Now.Hour == hora.Hour)
            {
                GravaLog("Iniciou a rotina", "debug");
                await EnviaMensagemClientesFinanceiro();
                GravaLog("Finalizou a rotina", "debug");
            }
            else
            {

            }
        }

        protected async Task<string> ServicoToken()
        {
            try
            {
                var post = new
                {
                    username = "UserAdmin",
                    password = ""
                };

                EventLog.WriteEntry($"Obtendo Token", EventLogEntryType.Warning);

                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("https://servicosapp.unimedribeirao.net/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = await client.PostAsJsonAsync("api/User/login", post);

                if (response.IsSuccessStatusCode)
                {
                    Token token = new Token();
                    token = await response.Content.ReadAsAsync<Token>();
                    GravaLog($"{token.token}","token");
                    return token.token;
                }

                return null;
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry($"{ex.Message}", EventLogEntryType.Error);
                throw ex;
            }
        }
        protected async Task EnviaMensagemClientesFinanceiro()
        {
            try
            {
                var token = await this.ServicoToken();
                var client = new RestClient("https://servicosapp.unimedribeirao.net/api/Clientes/EnviarMensagens");
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Authorization", "Bearer " + token);
                IRestResponse response = client.Execute(request);
                this.GravaLog($"sucesso {response.Content}", "sucesso");

            }
            catch (Exception ex)
            {
                EventLog.WriteEntry($"{ex.Message}", EventLogEntryType.Error);
                this.GravaLog($"{ex.Message}", "err");
                throw;
            }
        }
        //protected async Task EnviaMensagemClientesFinanceiro()
        //{
        //    try
        //    {
        //        var token = await this.ServicoToken();
        //        HttpClient client = new HttpClient();
        //        client.BaseAddress = new Uri("https://servicosapp.unimedribeirao.net/");
        //        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        //        var response = await client.GetAsync("api/Clientes/EnviaMensagem");

        //        if (response.IsSuccessStatusCode)
        //        {
        //            EventLog.WriteEntry($"{response.Content}", EventLogEntryType.Warning);
        //            this.GravaLog($"sucesso {DateTime.Now}", "sucesso");
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        EventLog.WriteEntry($"{ex.Message}", EventLogEntryType.Error);
        //        this.GravaLog($"{ex.Message}", "err");
        //        throw;
        //    }
        //}

        internal void GravaLog(string pMensagem, string pNomeArquivo)
        {
            try
            {
                string folders = AppDomain.CurrentDomain.BaseDirectory + "ApiServicoApp\\";

                if (!Directory.Exists(folders))
                    Directory.CreateDirectory(folders);

                string caminho = AppDomain.CurrentDomain.BaseDirectory + $"ApiServicoApp\\{pNomeArquivo}.txt";

                StreamWriter vWriter = new StreamWriter(caminho, true);

                vWriter.WriteLine($"{pMensagem}");
                vWriter.Flush();
                vWriter.Close();
            }
            catch (Exception)
            {
                //throw ex;
            }
            
        }

    }
    enum tempo : int
    {
        Dia = 86400000,
        Hora = 3600000,
        Minuto = 60000,
        min_10 = 600000
    }

    // New-Service -Name "ServicoCobranca" -BinaryPathName C:\Users\matheus.reis\Documents\Matheus\"PROJETOS REALIZADOS"\"SERVICO PARA RODAR"\ServicoAppService\ServicoAppService\bin\Debug\ServicoAppService.exe
    // Remove-Service -Name "ServicoCobranca"
}