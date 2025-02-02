using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace httpValidarCpf
{
    public static class fnvalidacpf
    {
        [FunctionName("fnvalidacpf")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Iniciando a validação do CPF");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            if(data == null) 
            {
                return new BadRequestObjectResult("Por favor, envie um CPF válido.");
            }

            string cpf = data?.cpf;

            
            if (string.IsNullOrEmpty(cpf) || !IsValidCpf(cpf))
            {
                return new BadRequestObjectResult("CPF inválido.");
            }

            // Se o CPF for válido, continue com a lógica
            log.LogInformation("CPF válido.");
            return new OkObjectResult("CPF válido.");
        }

        private static bool IsValidCpf(string cpf)
        {
            if (cpf.Length != 11 || long.TryParse(cpf, out _) == false)
            {
                return false;
            }

            // Verifica se todos os dígitos são iguais
            bool allDigitsSame = true;
            for (int i = 1; i < cpf.Length && allDigitsSame; i++)
            {
                if (cpf[i] != cpf[0])
                {
                    allDigitsSame = false;
                }
            }

            if (allDigitsSame)
            {
                return false;
            }

            // Validação dos dígitos verificadores
            int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            string tempCpf = cpf.Substring(0, 9);
            int soma = 0;

            for (int i = 0; i < 9; i++)
            {
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];
            }

            int resto = soma % 11;
            if (resto < 2)
            {
                resto = 0;
            }
            else
            {
                resto = 11 - resto;
            }

            string digito = resto.ToString();
            tempCpf = tempCpf + digito;
            soma = 0;

            for (int i = 0; i < 10; i++)
            {
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];
            }

            resto = soma % 11;
            if (resto < 2)
            {
                resto = 0;
            }
            else
            {
                resto = 11 - resto;
            }

            digito = digito + resto.ToString();

            return cpf.EndsWith(digito);
        }
    }
}
