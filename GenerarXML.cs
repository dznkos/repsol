using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Timers;
using System.Configuration;
using FacturaBE;
using FacturaDL;
using System.Xml;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Windows.Forms;
using System.IO.Compression;
using FacturadorLives;


namespace FacturadorBL
{

    public class GenerarXML
    {
        public static void Main(string[] args)
        {
            System.Timers.Timer MainTimer = new System.Timers.Timer();
            MainTimer.Interval = 20000;
            MainTimer.Start();
            Console.WindowWidth = 128;


            Console.WriteLine(@"
                   .----.            _____     _        ____   _____    _   _    ____        _      ____      U  ___ u   ____
             _.'__       `.         |' ___|U  /'\  u U /'___| |_ ' _|U |'|u| |U |  _'\ u U  /'\  u |  _'\      \/'_ \/U |  _'\ u
     . - -( #) ( #)---/#\          U| |_  u \/ _ \/  \| | u     | |   \| |\| | \| |_) |/  \/ _ \/ /| | | |     | | | | \| |_) |/
 . '    @             /###\        \|  _|/  / ___ \   | |/__   /| |\   | |_| |  |  _ <    / ___ \ U| |_| |\.-,_| |_| |  |  _ <
 :                 ,    \### /      |_|    /_/   \_\   \____| u |_|U  <<\___/   |_| \_\  /_/   \_\ |____/ u \_)-\___/   |_| \_\
   `  -  ..__.-' _.- \##/           )(\\,-  \\    >>  _// \\  _// \\_(__) )(    //   \\_  \\    >>  |||_         \\     //   \\_
               `;_:      `         (__)(_/ (__)  (__)(__)(__)(__) (__)   (__)  (__)  (__)(__)  (__)(__)_)       (__)   (__)  (__)
             ??????                 ##########################################################################################
           /    \\                  ########################################################################################
       //           \\              --------------------------------------------------------------------------------------
        `-._______.-'                           FACTURADOR ELECTRONICO --               POWER BY S.M.P.R
         ___`. | .'___
     (______|______)
                             ((    RQS GLOBAL WORK     ))

                            ");

            Console.WriteLine();
            string Texto;
            Texto = Console.ReadLine();
            Iniciar();
        }

        public void DoRemovespace(string strFile)
        {
            string str = System.IO.File.ReadAllText(strFile);
            str = str.Replace("\n", "");
            str = str.Replace("\r", "");
            Regex regex = new Regex(@">\s*<");
            string cleanedXml = regex.Replace(str, "><");
            System.IO.File.WriteAllText(strFile, cleanedXml);

        }

        private static void Iniciar()
        {

            Factura Fact = new Factura();
            var Directorio = new DirectoryInfo(ConfigurationManager.AppSettings["RutaXml"]);
            //Carpeta= Carpeta + "\txt_sunat"

            foreach (DirectoryInfo d in Directorio.GetDirectories())
            {
                string[] Nombre = d.Name.Split('-');
                string ParteInicial = d.Name.Substring(26, 2);   // Nombre[0].Substring(0, 1);
                Console.WriteLine(ParteInicial);
                string[] N = d.Name.Split('-');
                string NombreArchivoXml;
                string[] NombreCarpeta;
                NombreArchivoXml = Nombre[0] + "-" + N[1] + "-" + N[2] + "-" + N[3];

                int cont_ = 1;
                NombreCarpeta = NombreArchivoXml.Split('-');
                foreach (string word in Nombre)
                {
                    switch (cont_)
                    {
                        case 1:
                            Fact.SerieDocu = word;
                            break;
                        case 2:
                            Fact.NumeroDocu = word;
                            break;
                        case 3:
                            Fact.RucDocu_emp = word;
                            break;
                        case 4:
                            Fact.codTipDoc = word;
                            break;
                    }
                    cont_ = cont_ + 1;

                }

                cont_ = 1;


                if (ParteInicial.Equals("01")) // Factura
                {
                    Factura(ConfigurationManager.AppSettings["RutaXml"], d.Name, Fact, "F");

                }
                if (ParteInicial.Equals("03")) // BOLETA
                {
                    Factura(ConfigurationManager.AppSettings["RutaXml"], d.Name, Fact, "B");

                }
                if (ParteInicial.Equals("07")) // Nota Credito / (factura)
                {
                    Factura(ConfigurationManager.AppSettings["RutaXml"], d.Name, Fact, "NC");
                }

                if (ParteInicial.Equals("09")) // Guía de remisión remitente (guía)
                {
                    Factura(ConfigurationManager.AppSettings["RutaXml"], d.Name, Fact, "GR");
                }
                if (ParteInicial.Equals("08")) // Guía de remisión remitente (guía)
                {
                    Factura(ConfigurationManager.AppSettings["RutaXml"], d.Name, Fact, "ND");
                }
                if (ParteInicial.Equals("31")) // Guía de remisión transportista
                {

                    Factura(ConfigurationManager.AppSettings["RutaXml"], d.Name, Fact, "GT");
                }
            }
        }

        public static void Factura(string ruta, string NombreArchivo, Factura Fact, string TipoDocumento)
        {
            string Path = "";
            // 'Abrir carpeta
            switch (TipoDocumento)
            {
                case "F":                
                    Path = ruta + "/" + NombreArchivo + "/" + NombreArchivo + ".fac";
                    break;
                case "B":
                    Path = ruta + "/" + NombreArchivo + "/" + NombreArchivo + ".bol";
                    break;

                case "NC":
                case "ND" :
                    if (Fact.SerieDocu.Substring(0, 1).Equals("F"))  {
                        Path = ruta + "/" + NombreArchivo + "/" + NombreArchivo + ".fac";
                    }
                    else {
                        Path = ruta + "/" + NombreArchivo + "/" + NombreArchivo + ".bol";
                    }
                    break;  
              
                case "GR":
                    Path = ruta + "/" + NombreArchivo + "/" + NombreArchivo + ".gre";
                    break;
            }

            string text = System.IO.File.ReadAllText(Path);
            string Nom_Archivo = System.IO.Path.GetFileName(Path);
            string[] words_ = Nom_Archivo.Split('-');
            Double[] Cantidades = new Double[100];
            string[] UM = new string[100];
            string[] CD_item = new string[100];
            string[] Dsc_Item = new string[100];
            string[] ValorVenta_item = new string[100];
            string[] Item_Clasificacion = new string[100];
            string[] PrecioNeto = new string[100];
            MontoALetrasDL MontoLetras = new MontoALetrasDL();


            int CanItemCabera = 1;
            string fic = "", Item = "", texto = "", textoItem = "";
            string[] NumeroDoc;
            // Dim NombreCarpeta As String() = Nom_Archivo.Split(".fac")
            string[] NombreCarpeta;
            NombreCarpeta = Nom_Archivo.Split('.');
            int cont_txt_item = 1;
            int cont_item = 1;

            switch (TipoDocumento)
            {
                case "F":
                    fic = ConfigurationManager.AppSettings["RutaXml"] + "/" + NombreCarpeta[0] + "/" + NombreArchivo + ".fac";
                    Item = ConfigurationManager.AppSettings["RutaXml"] + "/" + NombreCarpeta[0] + "/" + NombreArchivo + ".item";
                    break;
                case "ND":
                    if (Fact.SerieDocu.Substring(0, 1).Equals("F"))
                    {
                        fic = ConfigurationManager.AppSettings["RutaXml"] + "/" + NombreCarpeta[0] + "/" + NombreArchivo + ".fac";
                        Item = ConfigurationManager.AppSettings["RutaXml"] + "/" + NombreCarpeta[0] + "/" + NombreArchivo + ".item";
                    }
                    else
                    {
                        fic = ConfigurationManager.AppSettings["RutaXml"] + "/" + NombreCarpeta[0] + "/" + NombreArchivo + ".bol";
                        Item = ConfigurationManager.AppSettings["RutaXml"] + "/" + NombreCarpeta[0] + "/" + NombreArchivo + ".item";
                    }
                    break;

                case "NC":
                    if (Fact.SerieDocu.Substring(0, 1).Equals("F"))
                    {
                        fic = ConfigurationManager.AppSettings["RutaXml"] + "/" + NombreCarpeta[0] + "/" + NombreArchivo + ".fac";
                        Item = ConfigurationManager.AppSettings["RutaXml"] + "/" + NombreCarpeta[0] + "/" + NombreArchivo + ".item";
                    }
                    else {
                        fic = ConfigurationManager.AppSettings["RutaXml"] + "/" + NombreCarpeta[0] + "/" + NombreArchivo + ".bol";
                        Item = ConfigurationManager.AppSettings["RutaXml"] + "/" + NombreCarpeta[0] + "/" + NombreArchivo + ".item";
                    }
                    break;
                case "B":
                    fic = ConfigurationManager.AppSettings["RutaXml"] + "/" + NombreCarpeta[0] + "/" + NombreArchivo + ".bol";
                    Item = ConfigurationManager.AppSettings["RutaXml"] + "/" + NombreCarpeta[0] + "/" + NombreArchivo + ".item";
                    break;
                case "GR":
                    fic = ConfigurationManager.AppSettings["RutaXml"] + "/" + NombreCarpeta[0] + "/" + NombreArchivo + ".gre";
                    Item = ConfigurationManager.AppSettings["RutaXml"] + "/" + NombreCarpeta[0] + "/" + NombreArchivo + ".item";
                    break;
            }

            texto = File.ReadAllText(fic);
            textoItem = File.ReadAllText(Item);
            //int cont_txt =1;
            string[] text_read = texto.Split('|');
            texto = texto.Replace("\r\n", "^");
            textoItem = textoItem.Replace("\r\n", "^");
            string[] texto_Item = textoItem.Split('^');
            string[] texto_Cabecera = texto.Split('^');
            CanItemCabera = 1;
            int cont_ = 1;
            // cont_ = 1;

            foreach (var item in texto_Cabecera)
            {
                words_ = item.Split('|');
                foreach (string word in words_)
                {
                    switch (CanItemCabera)
                    {
                        case 1:
                            switch (cont_)
                            {
                                case 1: //fecha Emision
                                    String cadena;
                                    cadena = String.Format(word, "yyyy-mm-dd");

                                    if (word.Length == 10 & cadena == word)
                                    {
                                        Fact.FechaEmi = word;
                                    }
                                    else
                                    {
                                        Console.WriteLine();
                                        Console.Write(word);
                                        Console.WriteLine("#(X) Error _ Fecha ingresada ");
                                        return;
                                    }
                                    break;
                                case 2: //hora de emision
                                    //Fact.HoraDoc = words_txt;
                                    if (word.Length <= 11)
                                    {
                                        if (ValidarCampos.vHour(word, "") == true | word == "00:00:00.0z")
                                        {
                                            Fact.HoraDoc = word;
                                        }
                                        else
                                        {
                                            Console.WriteLine("#(X) Error _ Hora ingresada ");
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        Fact.HoraDoc = word;
                                        Console.WriteLine("#(X) Error _ Hora ingresada _ Longitud");
                                        Console.WriteLine();
                                        return;
                                    }
                                    break;
                                case 3: //catalogo 51 atributo
                                    //Fact.Cod_tip_ope = words_txt;
                                    if (word.IsNumericInt().Equals(true))
                                    {

                                        if (word.Length == 4)
                                        {
                                            Fact.Cod_tip_ope = word;
                                        }
                                        else
                                        {
                                            Console.WriteLine("#(X) Error _ codigo de catalogo ");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("  ");
                                        Console.WriteLine("#(X) Error _ codigo de catalogo ");
                                        Console.WriteLine();
                                        return;
                                    }
                                    break;
                                case 4: //Tipo de moneda
                                    if (word.IsNumericInt().Equals(false))
                                    {
                                        if (ValidarCampos.Val_tip_moneda_6(word).Equals(false))
                                        {
                                            Console.Write(word);
                                            Console.Write("#(X) Error _ Tipo de moneda");
                                            Console.WriteLine();
                                            return;
                                        }
                                        else
                                        {
                                            Fact.TipMone = word;
                                        }
                                    }
                                    else
                                    {
                                        Console.Write(word);
                                        Console.Write("#(X) Error _ Tipo de moneda");
                                        Console.WriteLine();
                                        return;
                                    }
                                    break;
                                case 5: //NOMBRE DE MONEDA
                                    //Fact.NombreMoneda = words_txt;
                                    if (word.IsNumericInt().Equals(false))
                                    {
                                        if (ValidarCampos.Val_nomb_tip_mon(word, Fact.TipMone).Equals(false))
                                        {
                                            Console.WriteLine("#(X) Error _ nombre y el tipo de moneda no coinciden");
                                            return;
                                        }
                                        else
                                        {
                                            Fact.NombreMoneda = word;
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("#(X) Error _ nombre de moneda ");
                                        return;
                                    }
                                    break;
                                case 6: //Subtotal
                                    //Fact.SubTotal = Convert.ToDouble(words_txt);
                                    //Fact.SubTotal = Math.Round(Fact.SubTotal, 2);

                                    if (word.IsNumericDble().Equals(true))
                                    {
                                        Fact.SubTotal = Convert.ToDouble(word);
                                        Fact.SubTotal = Math.Round(Fact.SubTotal, 2);
                                    }
                                    else
                                    {
                                        Console.WriteLine("#(X) Error _ Subtotal ");
                                        Console.WriteLine();
                                    }
                                    break;
                                case 7: //Descuento
                                    //Fact.Descu = Convert.ToDouble(words_txt);
                                    if (word.IsNumericDble().Equals(true))
                                    {
                                        if (Math.Round(Convert.ToDouble(word), 2) != Math.Round(0.00, 2))
                                        {
                                            Fact.SubTotal = Fact.SubTotal - Math.Round(Convert.ToDouble(word), 2);
                                            Fact.Descuento_Activo = true;
                                            Fact.Descu = Convert.ToDouble(word);
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("#(X) Error _ Descuento no válido");
                                    }
                                    break;

                                case 8: //Total
                                    if (word.IsNumericDble().Equals(true))
                                    {
                                        if (Fact.Descuento_Activo.Equals(true))
                                        {
                                            Fact.Igv = (Fact.SubTotal * 0.18);
                                            Fact.Total = Fact.Igv + Fact.SubTotal;
                                            Fact.Igv = Math.Round(Fact.Igv, 2);
                                            Fact.Total = Math.Round(Fact.Total, 2);
                                        }
                                        else
                                        {
                                            Fact.Igv = Math.Round((Fact.SubTotal * 0.18), 2);
                                            Fact.Total = Math.Round(Convert.ToDouble(word), 2);
                                        }
                                    }
                                    break;
                                case 9: // Razon Social
                                    if (word.Length != 0 & word.Length < 100)
                                        Fact.Razon_Social_Emp = word;
                                    break;

                                case 10: // Nombre_Comercial
                                    if (word.Length != 0 & word.Length < 100)
                                        Fact.Nombre_Comercial_Emp = word;
                                    break;

                                case 11: // Direccion_Empresa -- PENDIENTE VERIF
                                    if (word.Length != 0)
                                        Fact.Direccion_Empresa = word;
                                    break;

                                case 12: // Telefono Fijo Empresa - pendiente
                                    if (word.Length != 0)
                                    {
                                        Fact.TelFijoEmpre = word;
                                    }
                                    else
                                    {
                                        Fact.CeluEmpre = "";
                                    }
                                    break;

                                case 13: // Celular Empresa - pendiente
                                    if (word.Length != 0)
                                    {
                                        Fact.CeluEmpre = word;
                                    }
                                    else
                                    {
                                        Fact.CeluEmpre = "";
                                    }
                                    break;

                                case 14: // Fecha Vencimiento
                                    String fecv;
                                    fecv = String.Format(word, "yyyy-mm-dd");

                                    if (word.Length == 10 & fecv == word)
                                    {
                                        Fact.FechVenc = String.Format(word, "yyyy-mm-dd");
                                    }
                                    else
                                    {
                                        Console.WriteLine("#(X) Error _ fecha vencimiento");
                                    }
                                    break;
                                case 15 :// Para Nota Credito o Debito
                                    Fact.Serie_Vinculada_ND_O_NC=word;
                                    break;
                                case 16 :
                                    Fact.NumeroDocumento_Vinculada_ND_O_ND=word;
                                    break ;
                                case 17 :
                                    Fact.TipoDocumento_ND_O_ND=word;
                                    break;
                                case 18 :
                                    Fact.FECHA_Vinculada_ND_O_NC=word;
                                    break;
                                    

                            }
                            break;
                        case 2:
                            switch (cont_)
                            {
                                case 1: //Codigo Guia Remision
                                    Fact.Codigo_Guia_Remisi = word;
                                    Fact.Codigo_Guia_Remisi = Fact.Codigo_Guia_Remisi.Replace("vbCrLf", string.Empty);
                                    break;
                            }
                            break;
                        case 3:
                            switch (cont_)
                            {
                                case 1: // Tipo Docu Identidad Client
                                    //Fact.Cod_Docu_identi = word;
                                    //Fact.Cod_Docu_identi = Fact.Cod_Docu_identi.Replace("vbCrLf", string.Empty);                                    
                                    if (word.Length <= 4)
                                    {
                                        Fact.Cod_Docu_identi = word;
                                    }
                                    else
                                    {
                                        //Fact.Cod_Docu_identi = Fact.Cod_Docu_identi.Replace("vbCrLf", string.Empty); 
                                        Console.WriteLine();
                                        Console.WriteLine("#(X) Error _ Cod Doc identidad");
                                        return;
                                    }
                                    break;

                                case 2: // Numero Documento Identidad (RUC)
                                    if (word.Length != 0)
                                        Fact.NumerodocumenIdenti = word;

                                    //validacion
                                    if (ValidarCampos.Validar_CodIdent(Fact.Cod_Docu_identi))
                                    {
                                        String ValDocuIde = Fact.Cod_Docu_identi;

                                        if (Fact.NumerodocumenIdenti.IsNumericInt() | Fact.NumerodocumenIdenti != string.Empty)
                                        {
                                            // Tipo de operacion
                                            if (Fact.Cod_tip_ope == "0101") // Venta interna (0101)
                                            {

                                                if (Fact.NumerodocumenIdenti == "")
                                                {
                                                    Console.WriteLine();
                                                    Console.WriteLine("#(X) Error _ Numero ruc");
                                                    Console.WriteLine();
                                                    return;
                                                }
                                                // RUC
                                                if ((Fact.NumerodocumenIdenti.Length == 11 & ValDocuIde == "6") |
                                                    // DNI
                                                    (Fact.NumerodocumenIdenti.Length == 8 & ValDocuIde == "1") |
                                                    // Carnet de extranjeria
                                                    (Fact.NumerodocumenIdenti.Length == 12 & ValDocuIde == "4") |
                                                    // Pasaporte
                                                    (Fact.NumerodocumenIdenti.Length == 12 & ValDocuIde == "7") |
                                                    // Cedula Diplomática de identidad
                                                    (Fact.NumerodocumenIdenti.Length == 15 & ValDocuIde == "A"))
                                                {
                                                    Fact.NumerodocumenIdenti = word;
                                                }
                                                else
                                                {
                                                    Console.WriteLine();
                                                    Console.WriteLine("#(X) Error _  Numero de documento de identificacion ");
                                                    Console.WriteLine();
                                                    return;
                                                }
                                            }
                                            else if (Fact.Cod_tip_ope == "0200") // Exportación de Bienes(0200)
                                            {
                                                Fact.NumerodocumenIdenti = "-";

                                                Fact.SubTotal += Fact.Igv;

                                                Fact.Igv = 0.00;

                                                if (ValidarCampos.Validar_CodIdent(Fact.Cod_Docu_identi))
                                                {
                                                }
                                                else if (Fact.NumerodocumenIdenti.Length == 4 & ValDocuIde == "0")
                                                {
                                                }
                                                else
                                                {
                                                    Console.WriteLine();
                                                    Console.WriteLine("#(X) Error _  Numero de documento de identificacion");
                                                    Console.WriteLine();
                                                    return;
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine();
                                                Console.WriteLine("#(X) Error _ Codigo no registrado en el sistema.");
                                                Console.WriteLine();
                                                return;
                                            } // end tipo de operacion
                                        }
                                        else
                                        {
                                            Console.WriteLine();
                                            Console.WriteLine("#(X) Error _  Numero de documento de identificacion");
                                            Console.WriteLine();
                                        }
                                    } //end validate
                                    break;
                                case 3:  // Razon Social Cliente 
                                    if (word.Length != 0 & word.Length < 100)
                                        Fact.Razon_Social_Cli = word;
                                    break;

                                case 4: // Nombre Comercial Cliente
                                    if (word.Length != 0 & word.Length < 100)
                                        Fact.Nombre_Comercial_Cli = word;
                                    break;

                                case 5: // Direccion Cliente
                                    if (word.Length != 0 & word.Length < 100)
                                        Fact.Direccion_Cli = word;
                                    break;

                                case 6: // Codigo tipo Leyenda
                                    if (word.Length != 0)
                                        Fact.CodLeye = word.Replace("\r\n", string.Empty);
                                    break;

                                case 7: // Codigo de Tributos (an..3)
                                    if (word.Length != 0 & word.Length < 4)
                                    {
                                        Fact.CodTribu = word;
                                    }
                                    else
                                    {
                                        Console.WriteLine("#(X) Error _ Codigo Tributo - no Valido");
                                    }
                                    break;
                                case 8: //Nombre Tributo (an..6) ('IGV','ISC','EXP',etc)
                                    if (word.Length != 0 & word.Length < 7)
                                    {
                                        Fact.NomTributo = word;  // FALTA VALIDAR EL CODIGO                    
                                    }
                                    else
                                    {
                                        Console.WriteLine("#(X) Error _ Nombre Tributo - no Valido");
                                    }
                                    break;
                                case 9: // Tipo Code Atribu (an4) ('EXC','FRE','VAT','OTH',etc)
                                    if (word.Length != 0 & word.Length < 5)
                                    {
                                        Fact.TipeCodeAtribu = word;
                                    }
                                    else
                                    {
                                        Console.WriteLine("#(X) Error _ tipo code atribu");
                                    }
                                    break;

                                case 10: // Categoría  del  Impuesto  
                                    // nombre de afectación (an2) (TaxExemptionReasonCode)
                                    if (word.Length != 0 & word.Length < 100)
                                    {
                                        Fact.NomAfecta = word;
                                    }
                                    break;

                                case 11: // Codigo tipo de afectación del IGV (an2)???? - FALTA VALIDAR 
                                    if (word.Length != 0 & word.Length < 3)
                                    {
                                        Fact.codafecta = word;
                                    }
                                    else
                                    {
                                        Console.WriteLine("Error _ Codigo de tipo de afectación  - no Valido");
                                    }
                                    break;
                            }
                            break;

                        case 4:
                            switch (cont_)
                            {
                                case 1: // Codigo tipo Leyenda
                                    if (word.Length != 0)
                                        Fact.CodLeye = word.Replace("\r\n", string.Empty);
                                    break;

                                case 2: // Codigo de Tributo (an4) ('1000','2000',etc) 
                                    if (word.Length != 0 & word.Length < 5)
                                    {
                                        Fact.CodTribu = word;
                                    }
                                    else
                                    {
                                        Console.WriteLine("Error _ Codigo Tributo - no Valido");
                                    }
                                    break;

                                case 3: //Nombre Tributo (an..6) ('IGV','ISC','EXP',etc)
                                    if (word.Length != 0 & word.Length < 7)
                                    {
                                        Fact.NomTributo = word; // FALTA VALIDAR EL CODIGO                 
                                    }
                                    else
                                    {
                                        Console.WriteLine("#(X) Error _ Nombre Tributo - no Valido");
                                    }
                                    break;

                                case 4: //Tipo Code Atribu (an4) ('EXC','FRE','VAT','OTH',etc)
                                    if (word.Length != 0 & word.Length < 5) // FALTA VALIDAR EL CODIGO
                                    {
                                        Fact.TipeCodeAtribu = word;
                                        //Fact.CodInternacional = word;
                                    }
                                    else
                                    {
                                        Console.WriteLine("#(X) Error _ tipo code atribu");
                                    }
                                    break;

                                case 5: // Categoría  del  Impuesto                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            
                                    // Nombre de afectación (an??) - FALTA VALIDAR                                  
                                    if (word.Length != 0 & word.Length < 100)
                                    {
                                        Fact.CatImpuesto = word;
                                    }
                                    break;

                                case 6: // Descripcion tipo de afectación del IGV - FALTA VALIDAR 
                                    if (word.Length != 0 & word.Length < 100)
                                    {
                                        Fact.NomAfecta = word;
                                    }
                                    else
                                    {
                                        Console.WriteLine("#(X) Error _ Codigo de tipo de afectación  - no Valido");
                                    }
                                    break;

                                case 7: // Codigo tipo de afectación del IGV (an2)???? - FALTA VALIDAR 
                                    if (word.Length != 0 & word.Length < 3)
                                    {
                                        Fact.codafecta = word;
                                    }
                                    else
                                    {
                                        Console.WriteLine("#(X) Error _ Codigo de tipo de afectación  - no Valido");
                                    }
                                    break;

                                case 8: // CANTIDAD BOLSAS
                                    
                                    if (word.IsNumericInt().Equals(true))
                                    {
                                        Fact.CantidadBolsa = Convert.ToDouble(word);
                                    }
                                    break;

                                case 9: // DESCRIPCION DE TRIBUTO
                                    if (word.IsNumericDble().Equals(true))
                                    {
                                        Fact.PrecioUnitBolsa = Convert.ToDouble(word);
                                    }
                                    break;

                                case 10: // IMPORTE DE BOLSA
                                    if (word.IsNumericDble().Equals(true))
                                    {
                                        Fact.ImporteBolsa = Convert.ToDouble(word);
                                    }
                                    break;
                                case 11: // IMPORTE DE BOLSA + IMPORTE DEL IMPUESTO
                                    if (word.IsNumericDble().Equals(true))
                                    {
                                        Fact.ImporteTotalBolsa = Convert.ToDouble(word);
                                    }
                                    break;
                            }
                            break;
                    }
                    cont_ = cont_ + 1;
                }
                cont_ = 1;
                CanItemCabera = CanItemCabera + 1;
            }
            cont_ = 1;

            double percent = 18.0;
            double igv_item, ValorUnitario;

            cont_ = 1;

            foreach (string words_item in texto_Item)
            {
                if (!string.IsNullOrEmpty(words_item))
                {

                    string[] Texto_Item_separado = words_item.Split('|');
                    foreach (string words in Texto_Item_separado)
                    {
                        switch (cont_)
                        {
                            case 1:
                                Cantidades[cont_txt_item] = Convert.ToDouble(words);
                                break;
                            case 2:
                                UM[cont_txt_item] = words;
                                break;
                            case 3:
                                CD_item[cont_txt_item] = words;
                                break;
                            case 4:
                                Dsc_Item[cont_txt_item] = words;
                                break;
                            case 5:
                                ValorVenta_item[cont_txt_item] = words;
                                break;
                            case 6:
                                Item_Clasificacion[cont_txt_item] = words;
                                break;
                            case 7:
                                Item_Clasificacion[cont_txt_item] = words;
                                break;

                        }
                        cont_ = cont_ + 1;
                    }
                    cont_ = 1;
                    cont_txt_item = cont_txt_item + 1;
                }

            }
            cont_ = 1;



            string[] N = NombreArchivo.Split('-');
            string NombreArchivoXml = N[2] + "-" + N[3] + "-" + N[0] + "-" + N[1] + ".xml";

            string path = ConfigurationManager.AppSettings["RutaXml"] + "/" + NombreArchivoXml;

            Fact.Monto_Letras = MontoLetras.MontoALetras(Fact.Total.ToString(), "");
            //Inicio
            var xmlNacional = "<?xml version='1.0' encoding='ISO-8859-1'?> ";
            switch (TipoDocumento)
            {
                case "F":
                case "B":
                    xmlNacional = xmlNacional
                    + "<Invoice xmlns='urn:oasis:names:specification:ubl:schema:xsd:Invoice-2' ";
                    break;
                case "NC":
                    xmlNacional = xmlNacional
                   + " <CreditNote xmlns='urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2' ";
                    break;
                case "ND":
                    xmlNacional = xmlNacional
                  + " <DebitNote xmlns='urn:oasis:names:specification:ubl:schema:xsd:DebitNote-2'";
                    break;
                case "GR":
                    xmlNacional = xmlNacional
                   + " <DespatchAdvice xmlns='urn:oasis:names:specification:ubl:schema:xsd:DespatchAdvice-2' ";
                    break;
            }
            xmlNacional = xmlNacional
          + " xmlns:cac='urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2'"
          + " xmlns:cbc='urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2'"
          + " xmlns:ccts='urn:oasis:names:specification:ubl:schema:xsd:CoreComponentParameters-2'"
                //+ " xmlns:ccts='urn:un:unece:uncefact:documentation:2'"
          + " xmlns:ds='http://www.w3.org/2000/09/xmldsig#'"
          + " xmlns:ext='urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2'"
          + " xmlns:qdt='urn:oasis:names:specification:ubl:schema:xsd:QualifiedDatatypes-2'"
          + " xmlns:sac='urn:sunat:names:specification:ubl:peru:schema:xsd:SunatAggregateComponents-1'"
          + " xmlns:stat='urn:oasis:names:specification:ubl:schema:xsd:DocumentStatusCode-1.0'"
          + " xmlns:udt='urn:un:unece:uncefact:data:draft:UnqualifiedDataTypesSchemaModule:2'"
          + " xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";


            xmlNacional = xmlNacional
                //  ====  FIRMA  ====
             + " <ext:UBLExtensions> "
             + " <ext:UBLExtension> "
             + " <ext:ExtensionContent> "
             + " </ext:ExtensionContent> "
             + " </ext:UBLExtension> "
             + " </ext:UBLExtensions>"
             + " <cbc:UBLVersionID>2.1</cbc:UBLVersionID> "; //Obligatorio--ND (obligatorio)

             if (TipoDocumento == "GR"){
                 xmlNacional = xmlNacional
                 + " <cbc:CustomizationID>1.0</cbc:CustomizationID> "; //Obligatorio   1.-
             }
             else {
                 xmlNacional = xmlNacional
                 + " <cbc:CustomizationID>2.0</cbc:CustomizationID> "; //Obligatorio --ND (obligatorio)  2.- 
             }
             
            //switch (TipoDocumento)
            //{
            //    case "F":
            //        xmlNacional = xmlNacional
            //        //+ " <cbc:ProfileID "
            //        //+ " schemeName='SUNAT: Identificador de Tipo de Operación' "
            //        //+ " schemeAgencyName='PE: SUNAT' "
            //        //+ " schemeURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo17'>" + Fact.Cod_tip_ope + "</cbc:ProfileID> "; // Tipo Operacion - Obligatorio         
            //        //break;
            //}
            
            xmlNacional = xmlNacional
            + " <cbc:ID>" + Fact.SerieDocu + "-" + Fact.NumeroDocu + "</cbc:ID> "//ND (obligatorio) 3-Numero(serie - Numero Correlativo)
            + " <cbc:IssueDate>" + Fact.FechaEmi + "</cbc:IssueDate> "//ND (obligatorio) 4- Fecha Emision
            + " <cbc:IssueTime>" + Fact.HoraDoc + "</cbc:IssueTime> "; // ND Hora Emision 5 
            switch (TipoDocumento)
            {
                case "F":
                case "B":
                    xmlNacional = xmlNacional
                    + " <cbc:InvoiceTypeCode listAgencyName='PE:SUNAT' listID='"
                      // listName='SUNAT:Identificador de Tipo de Documento'"
                    + Fact.Cod_tip_ope + "' listName='Tipo de Documento'"
                    + " listSchemeURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo51'"
                    + " listURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo01' name='Tipo de Operacion'>"
                    + Fact.codTipDoc + "</cbc:InvoiceTypeCode>";
                    break;
                case "NC":
                    //if (Fact.SerieDocu.Substring(0, 1).Equals("B"))
                    //{  // Boleta
                    //   xmlNacional = xmlNacional
                    //   + "<cbc:InvoiceTypeCode listAgencyName='PE:SUNAT' listName='Tipo de Documento' listURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo01'>07</cbc:InvoiceTypeCode>";
                    //}
                    //else if (Fact.SerieDocu.Substring(0, 1).Equals("F"))
                    //{  // Factura
                    //    xmlNacional = xmlNacional
                    //    + "<cbc:InvoiceTypeCode listAgencyName='PE:SUNAT' listName='Tipo de Documento' listURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo01'>07</cbc:InvoiceTypeCode>";
                    //}                     
                    break;
                case "GR":
                    xmlNacional = xmlNacional
                    + "<cbc:DespatchAdviceTypeCode>" + Fact.codTipDoc + "</cbc:DespatchAdviceTypeCode>";
                    break;
            }

            // Note_Leyenda (OPCIONAL)
            switch (TipoDocumento)
            {
                case "F":
                case "B":
                case "ND":
                    xmlNacional = xmlNacional
                      //+ "<cbc:Note languageLocaleID='" + Fact.CodLeye + "'>0501002017062500125</cbc:Note>";
                    + " <cbc:Note languageLocaleID='" + Fact.CodLeye + "'><![CDATA[" + Fact.Monto_Letras + " " + Fact.NombreMoneda + "]]></cbc:Note>"
                      // + "<cbc:Note><![CDATA[" + Fact.Observacion + "]]></cbc:Note>";
                    + " <cbc:DocumentCurrencyCode listAgencyName='United Nations Economic Commission for Europe' listID='ISO 4217 Alpha' listName='Currency'>"
                    + Fact.TipMone + "</cbc:DocumentCurrencyCode>"; //Tipo Moneda ND (Obligatorio) 8
                    break;
               
               }
           

            switch (TipoDocumento)
            {
                case "F":
                    //xmlNacional = xmlNacional
                    //Número de guía de remisión relacionada con la operación que se factura
                    //+ " <cac:DespatchDocumentReference>"
                    //+ " <cbc:ID>" + Fact.Codigo_Guia_Remisi + "</cbc:ID> "//Tipo Y Numero Guia de remision
                    //+ " <cbc:DocumentTypeCode listAgencyName='PE:SUNAT'  listName='SUNAT:Identificador de guía relacionada' listURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo01'>09</cbc:DocumentTypeCode> "
                    //+ " </cac:DespatchDocumentReference> ";
                    break;
                case "B":
                   // xmlNacional = xmlNacional
                   //+ " <cbc:DocumentCurrencyCode>"
                   //+ Fact.TipMone + "</cbc:DocumentCurrencyCode> ";//Tipo Moneda
                    break;
                case "NC":
                case "ND":
                    xmlNacional = xmlNacional
                    + "<cac:DiscrepancyResponse>"
                    + "<cbc:ReferenceID>" + Fact.Serie_Vinculada_ND_O_NC + "-" + Fact.NumeroDocumento_Vinculada_ND_O_ND + "</cbc:ReferenceID>"
                    + "<cbc:ResponseCode listName='Tipo de nota de debito' listAgencyName='PE:SUNAT' listSchemeURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo10' >03</cbc:ResponseCode>"//Motivo de Nota debito //Penalidades/ otros conceptos (OBLIGATORIO) Tipo nota de debito
                    + "<cbc:Description><![CDATA[PRUEBA]]></cbc:Description>" //Obligatorio (ND) Motivo o Sustento
                    + "</cac:DiscrepancyResponse>"
                    + "<cac:BillingReference>"
                    + "<cac:InvoiceDocumentReference>"
                    + "<cbc:ID>" + Fact.Serie_Vinculada_ND_O_NC + "-" + Fact.NumeroDocumento_Vinculada_ND_O_ND + "</cbc:ID>" //17 Serie y Numero (Modifica) Obligatorio 
                    + "<cbc:IssueDate>" + Fact.FECHA_Vinculada_ND_O_NC + "</cbc:IssueDate>"
                    + "<cbc:DocumentTypeCode listAgencyName='PE:SUNAT' listName='Tipo de Documento' listURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo01'>" + Fact.TipoDocumento_ND_O_ND + "</cbc:DocumentTypeCode>" //18 Tipo documento Modifica
                    + "</cac:InvoiceDocumentReference>"
                    + "</cac:BillingReference>";
                    break;
                case "GR":
                      //Despatch Advice. Order Reference
                      //A reference to an Order with which this Despatch Advice is associated.		
                      //Se usa para asignar un gu�a dada de baja - Gu�a de Remisi�n dada de Baja 
                      // Cat�logo N� 01 debe ser = 09 -->
                      xmlNacional = xmlNacional
                      //<#if idDocBaja??>
                      + "<cac:OrderReference>"

                      //+ "<cbc:ID>${idDocBaja}</cbc:ID>"
                      + "<cbc:ID>T001-8</cbc:ID>"   
                   
                        //<cbc:SalesOrderID>CON0095678</cbc:SalesOrderID>
                        //+ "<cbc:UUID>6E09886B-DC6E-439F-82D1-7CCAC7F4E3B1</cbc:UUID>"
                        //+ "<cbc:IssueDate>2005-06-20</cbc:IssueDate>"
                        
                        // (GUIA (OrderTypeCode)= OPCIONAL)
                      //+ "<cbc:OrderTypeCode name='${tipDocBaja}'>${codTipDocBaja}</cbc:OrderTypeCode>"
                      + "<cbc:OrderTypeCode>09</cbc:OrderTypeCode>"                      
                      + "</cac:OrderReference>";
                    break;

                 
            }


            xmlNacional = xmlNacional
             //Informacion adicional de Firma
           + "<cac:Signature>"
           + "<cbc:ID>" + Fact.SerieDocu + "-" + Fact.NumeroDocu + "</cbc:ID> " //Preguntar el campo
           + "<cac:SignatoryParty>"
           + "<cac:PartyIdentification>"
           + "<cbc:ID>" + Fact.RucDocu_emp + "</cbc:ID>"
           + "</cac:PartyIdentification>";
            xmlNacional = xmlNacional
            + "<cac:PartyName >"
            + "<cbc:Name>" + Fact.Razon_Social_Emp + "</cbc:Name>"
            + "</cac:PartyName>"
            + "</cac:SignatoryParty>"
            + "<cac:DigitalSignatureAttachment>"
            + "<cac:ExternalReference>"
            + "<cbc:URI>#sign" + Fact.Nombre_Comercial_Emp + "</cbc:URI>" //Pregunta el campo
            + "</cac:ExternalReference>"
            + "</cac:DigitalSignatureAttachment>"
            + "</cac:Signature>";


            // ====  PROVEEDOR  ==== 
            switch(TipoDocumento){
                case "GR":
                    {
                        //Cabecera - Datos del Remitente
                        //Numero de documento de identidad del remitente
                        //Tipo de documento de identidad del remitente
                        //Apellidos y nombres, denominaci�n o raz�n social del remitente 
                        //Despatch Advice. Despatch_ Supplier Party. Supplier Party 
                         xmlNacional = xmlNacional
                        + "<cac:DespatchSupplierParty>"
                        + "<cbc:CustomerAssignedAccountID schemeID='6'>20262520243</cbc:CustomerAssignedAccountID>"
                        + "<cbc:AdditionalAccountID/>"
                        + "<cac:Party>"
                        + "<cac:PartyName>"
                        + "<cbc:Name><![CDATA[" + Fact.Nombre_Comercial_Emp + "]]></cbc:Name>"
                        + "</cac:PartyName>"
                        + "<cac:PartyLegalEntity>"
                        + "<cbc:RegistrationName><![CDATA[" + Fact.Nombre_Comercial_Emp + "]]></cbc:RegistrationName>"
                        + "</cac:PartyLegalEntity>"
                        + "</cac:Party>"
                        + "</cac:DespatchSupplierParty>";
                        break;
                    
                    }
                case "B":
                case "F":
                case "NC":
                    xmlNacional = xmlNacional
                  + "<cac:AccountingSupplierParty>"
                  + "<cbc:CustomerAssignedAccountID>" + Fact.NumeroDocu + "</cbc:CustomerAssignedAccountID>"
                  + "<cbc:AdditionalAccountID>6</cbc:AdditionalAccountID>"
                  + "<cac:Party>"
                        // No aparece en ubl 2.1 //
                  + "<cac:PartyIdentification>"
                  + "<cbc:ID schemeAgencyName='PE:SUNAT' schemeID='6'  schemeName='Documento de Identidad' schemeURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo06'>" + Fact.RucDocu_emp + "</cbc:ID>"
                  + "</cac:PartyIdentification>"; 
                    break;
                case "ND":
                    xmlNacional = xmlNacional
                + "<cac:AccountingSupplierParty>"
                + "<cac:Party>"
                + "<cac:PartyIdentification>"
                + "<cbc:ID schemeAgencyName='PE:SUNAT' schemeID='6'  schemeName='Documento de Identidad' schemeURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo06'>" + Fact.RucDocu_emp + "</cbc:ID>"
                + "</cac:PartyIdentification>"; //Nota de credito (10) Datos del Emisor  - Ruc
                  
                break;
                
            }

            

            switch (TipoDocumento)
            {
                case "F":
                    xmlNacional = xmlNacional
                    + "<cac:PartyName>"
                    + "<cbc:Name><![CDATA[" + Fact.Nombre_Comercial_Emp + "]]></cbc:Name>"
                    + "</cac:PartyName>"
                    + "<cac:PartyLegalEntity>"
                    + "<cbc:RegistrationName><![CDATA[" + Fact.Razon_Social_Emp + "]]></cbc:RegistrationName>"
                    + "<cac:RegistrationAddress>"
                    + "<cbc:AddressTypeCode listAgencyName='PE:SUNAT' listName='Establecimientos anexos'>0</cbc:AddressTypeCode>"
                    + "<cbc:CitySubdivisionName/>"
                    + "<cbc:CityName/>"
                    + "<cbc:CountrySubentity/>"
                    + "<cbc:CountrySubentityCode/>"
                    + "<cbc:District/>"
                    + "<cac:AddressLine>"
                      // VERIFICAR DIRECCIÓN ESTABLECIDA
                    + "<cbc:Line><![CDATA[AV. LUNA PIZARRO 336 URB. TEJADA ALTA A 1/2 CDRA. DE OVALO DE BALTA]]></cbc:Line>"
                    + "</cac:AddressLine>"
                    + "<cac:Country>"
                    + "<cbc:IdentificationCode>PE</cbc:IdentificationCode>"
                    + "</cac:Country>"
                    + "</cac:RegistrationAddress>"
                    + "</cac:PartyLegalEntity>";
                    break;
                case "B":
                case "NC":
                case "ND" :
                    xmlNacional = xmlNacional
                   + "<cac:PartyName>"
                   + "<cbc:Name><![CDATA[" + Fact.Nombre_Comercial_Emp + "]]></cbc:Name>"  //11.- NOTA DE DEBITO  Nombre Comercial (Emisor)
                   + "</cac:PartyName>"
                   + "<cac:PartyLegalEntity>"
                   + "<cbc:RegistrationName><![CDATA[" + Fact.Razon_Social_Emp + "]]></cbc:RegistrationName>"// 12 Obligatorio .- razón social o Apellido Nombre 
                   + "<cac:RegistrationAddress>"
                   + "<cbc:ID>150101</cbc:ID>"
                 //  + "<cbc:BuildingNumber />"
                 + "<cbc:AddressTypeCode>0</cbc:AddressTypeCode>"//Codigo asignado por sunat para el establecimiento anexo declarado en el ruc 14 (Obligatorio)
                   + "<cbc:CitySubdivisionName>-</cbc:CitySubdivisionName>"
                   + "<cbc:CityName><![CDATA[LIMA]]></cbc:CityName>"
                   + "<cbc:CountrySubentity><![CDATA[LIMA]]></cbc:CountrySubentity>"
                  // + "<cbc:CountrySubentityCode><![CDATA[150104]]></cbc:CountrySubentityCode>"
                   + "<cbc:District><![CDATA[BARRANCO]]></cbc:District>"
                   + "<cac:AddressLine>"
                   + "<cbc:Line><![CDATA[" + Fact.Direccion_Empresa + "]]></cbc:Line>" //Direccion emisor
                   + "</cac:AddressLine>"
                   + "<cac:Country>"
                   + "<cbc:IdentificationCode>PE</cbc:IdentificationCode>"
                   + "</cac:Country>"
                   + "</cac:RegistrationAddress>"
                   + "</cac:PartyLegalEntity>";
                    break;                  

            }

            //Si los Campos vacion colocar - (codigo documento identidad)
            if (string.IsNullOrEmpty(Fact.NumerodocumenIdenti))
            {
                Fact.NumerodocumenIdenti = "-";
            }


            switch (TipoDocumento)
            {
                case "B":
                case "F":
                case "NC":
                case "ND":
                        xmlNacional = xmlNacional
                        + "</cac:Party>"
                        + "</cac:AccountingSupplierParty>"
                        //=======================================================
                        //======================  CLIENTE  ====================== 
                        + "<cac:AccountingCustomerParty>"
                        + "<cac:Party>"
                        + "<cac:PartyIdentification>"
                           // schemeID='" + Fact.Cod_Docu_identi.Trim() + "'
                        + "<cbc:ID schemeAgencyName='PE:SUNAT' schemeID='-' schemeName='Documento de Identidad' schemeURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo06'>" + Fact.NumerodocumenIdenti + "</cbc:ID>" //Tipo y numero de documento de identidad del cliente o receptor (Obligatorio) 
                        + "</cac:PartyIdentification>"
                        + "<cac:PartyLegalEntity>"
                        + "<cbc:RegistrationName><![CDATA[" + Fact.Nombre_Comercial_Cli + "]]></cbc:RegistrationName>" // Razon social  del cliente (Nota de debito 16) 
                        + "</cac:PartyLegalEntity>"
                        + "</cac:Party>"
                        + "</cac:AccountingCustomerParty>";
                        break;

                case "GR":
                        //<!-- Cabecera - Datos del Destinatario
                        //Numero de documento de identidad del destinatario
                        //Tipo de documento de identidad
                        //Apellidos y nombres, denominaci�n o raz�n social del destinatario 
                        //Despatch Advice. Delivery_ Customer Party. Customer Party -->
                        xmlNacional = xmlNacional
                        + "<cac:DeliveryCustomerParty>"
                        + "<cbc:CustomerAssignedAccountID schemeID='6'>10209865209</cbc:CustomerAssignedAccountID>"
                        + "<cbc:AdditionalAccountID/>"
                        + "<cac:Party>"
                        + "<cac:PartyName>"
                        + "<cbc:Name><![CDATA[" + Fact.Nombre_Comercial_Cli  + "]]></cbc:Name>"
                        + "</cac:PartyName>"
                        + "<cac:PartyLegalEntity>"
                        + "<cbc:RegistrationName><![CDATA[" + Fact.Razon_Social_Cli + "]]></cbc:RegistrationName>"
                        + "</cac:PartyLegalEntity>"
                        + "</cac:Party>"
                        + "</cac:DeliveryCustomerParty>";
                        break;
            }
            
            

            switch (TipoDocumento)
            {

                case "F":
                    xmlNacional = xmlNacional
                        + "<cac:TaxTotal>"
                        + "<cbc:TaxAmount currencyID='" + Fact.TipMone + "'>" + String.Format("{0:0.00}", Fact.Igv) + "</cbc:TaxAmount>" //<!--igv-->
                        //<!-- 1000 Total valor de venta - operaciones gravadas. + SUMATORIA ISC-->
                        + "<cac:TaxSubtotal>"
                        + "<cbc:TaxableAmount currencyID='" + Fact.TipMone + "'>" + String.Format("{0:0.00}", Fact.Total) + "</cbc:TaxableAmount>"
                        + "<cbc:TaxAmount currencyID='" + Fact.TipMone + "'>" + String.Format("{0:0.00}", Fact.Igv) + "</cbc:TaxAmount>"
                        //+ " <cbc:TaxAmount currencyID='" + Fact.TipMone + "'>" + String.Format("{0:0.00}",Fact.Igv) + "</cbc:TaxAmount>"
                        + "<cac:TaxCategory>"
                        //<cbc:ID schemeAgencyName="United Nations Economic Commission for Europe" schemeID="UN/ECE 5305" schemeName="Tax Category Identifier">S</cbc:ID>
                        + "<cbc:ID schemeAgencyName='United Nations Economic Commission for Europe' schemeID='UN/ECE 5305' schemeName='Tax Category Identifier'>S</cbc:ID>"
                        + "<cac:TaxScheme>"
                        + "<cbc:ID schemeAgencyName='PE:SUNAT' schemeID='UN/ECE 5153' schemeName='Codigo de tributos'>" + Fact.CodTribu + "</cbc:ID>"//Codigo Internacional Tributo 
                        + "<cbc:Name>" + Fact.NomTributo + "</cbc:Name>"//Nombre de tributo// FACTURA - EXP // BOLETA -ISC 
                        + "<cbc:TaxTypeCode>" + Fact.TipeCodeAtribu + "</cbc:TaxTypeCode>"//Nombre de tributo// FACTURA - FRE // BOLETA -EXC      //
                        + "</cac:TaxScheme>"
                        + "</cac:TaxCategory>"
                        + "</cac:TaxSubtotal>"
                        + "</cac:TaxTotal>";
                    break;

                case "B":
                    xmlNacional = xmlNacional
                        // Impuesto tributo (ICBPER) Bolsas -----
                            + "<cac:TaxTotal>"
                            + "<cbc:TaxAmount currencyID='" + Fact.TipMone + "'>" + (Fact.Igv + Fact.ImporteBolsa).ToString() + "</cbc:TaxAmount>"
                            //+ "<cbc:TaxAmount currencyID='" + Fact.TipMone + "'>" + String.Format("{0:0.00}", Fact.SubTotal) + "</cbc:TaxAmount>"//<cbc:TaxAmount currencyID="PEN">3.05</cbc:TaxAmount> //Nota el valor por defecto 3.05

                        // Fin Impuesto tributo 

                            // Totales
                        //ISC(IMPUESTO SELECTIVO CONSUMO)
                        //    + "<cbc:TaxAmount currencyID='" + Fact.TipMone + "'>0.00</cbc:TaxAmount>"
                        //    + "<cac:TaxSubtotal>"
                        //    + "<cbc:TaxAmount currencyID='" + Fact.TipMone + "'>0.00</cbc:TaxAmount>"
                        //    + "<cac:TaxCategory>"
                        //    + "<cac:TaxScheme>"
                        //    + "<cbc:ID>2000</cbc:ID>" //Codigo Internacional Tributo 
                        //    + "<cbc:Name>ISC</cbc:Name>"
                        //    + "<cbc:TaxTypeCode>EXC</cbc:TaxTypeCode>"
                        //    + "</cac:TaxScheme>"
                        //    + "</cac:TaxCategory>"
                        //    + "</cac:TaxSubtotal>"


                            + "<cac:TaxSubtotal>"
                            + "<cbc:TaxAmount currencyID='" + Fact.TipMone + "'>" + String.Format("{0:0.00}", (Fact.PrecioUnitBolsa * Fact.CantidadBolsa)) + "</cbc:TaxAmount>"
                            + "<cac:TaxCategory>"
                            + "<cac:TaxScheme>"
                            + "<cbc:ID schemeAgencyName='PE:SUNAT' schemeName='Codigo de tributos' schemeURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo05'>7152</cbc:ID>"
                            + "<cbc:Name>ICBPER</cbc:Name>"
                            + "<cbc:TaxTypeCode>OTH</cbc:TaxTypeCode>"
                            + "</cac:TaxScheme>"
                            + "</cac:TaxCategory>"
                            + "</cac:TaxSubtotal>"

                            //IGV
                          //  + "<cbc:TaxAmount currencyID='" + Fact.TipMone + "'>" + String.Format("{0:0.00}", Fact.SubTotal) + "</cbc:TaxAmount>"//<cbc:TaxAmount currencyID="PEN">3.05</cbc:TaxAmount> //Nota el valor por defecto 3.05
                            + "<cac:TaxSubtotal>"
                            + "<cbc:TaxableAmount currencyID='" + Fact.TipMone + "'>" + String.Format("{0:0.00}", Fact.SubTotal) + "</cbc:TaxableAmount>"//<cbc:TaxAmount currencyID="PEN">3.05</cbc:TaxAmount> //Nota el valor por defecto 3.05
                            + "<cbc:TaxAmount currencyID='" + Fact.TipMone + "'>" + String.Format("{0:0.00}", Fact.Igv) + "</cbc:TaxAmount>"
                            + "<cac:TaxCategory>"
                            + "<cac:TaxScheme>"
                            + "<cbc:ID schemeAgencyName='PE:SUNAT' schemeID='UN/ECE 5153' schemeName='Codigo de tributos'>" + Fact.CodTribu + "</cbc:ID>" //Codigo Internacional Tributo 
                            + "<cbc:Name>" + Fact.NomTributo + "</cbc:Name>"
                            + "<cbc:TaxTypeCode>" + Fact.TipeCodeAtribu + "</cbc:TaxTypeCode>"
                            + "</cac:TaxScheme>"
                            + "</cac:TaxCategory>"
                            + "</cac:TaxSubtotal>"

                        // + "</cac:TaxTotal>"

                                // OTROS TRIBUTOS
                        // + "<cac:TaxTotal>"
                            //+ "<cbc:TaxAmount currencyID='" + Fact.TipMone + "'>0.00</cbc:TaxAmount>"
                            //+ "<cac:TaxSubtotal>"
                            //+ "<cbc:TaxAmount currencyID='" + Fact.TipMone + "'>0.00</cbc:TaxAmount>"
                            //+ "<cac:TaxCategory>"
                            //+ "<cac:TaxScheme>"
                            //+ "<cbc:ID>9999</cbc:ID>"
                            //+ "<cbc:Name>OTROS TRIBUTOS</cbc:Name>"
                            //+ "<cbc:TaxTypeCode>OTH</cbc:TaxTypeCode>"
                            //+ "</cac:TaxScheme>"
                            //+ "</cac:TaxCategory>"
                            //+ "</cac:TaxSubtotal>"
                            + "</cac:TaxTotal>";
                    break;


                case "NC":
                case "ND":
                    xmlNacional = xmlNacional
                    + "<cac:TaxTotal>";
                    if (Fact.SerieDocu.Substring(0, 1).Equals("F"))
                    {//Factura
                        xmlNacional = xmlNacional
                        + "<cbc:TaxAmount currencyID='" + Fact.TipMone + "'>0.00</cbc:TaxAmount>"
                        + "<cac:TaxSubtotal>"
                        + "<cbc:TaxableAmount currencyID='" + Fact.TipMone + "'>" + String.Format("{0:0.00}", Fact.SubTotal) + "</cbc:TaxableAmount>"
                        + "<cbc:TaxAmount currencyID='" + Fact.TipMone + "'>0.00</cbc:TaxAmount>";
                    }
                    else if (Fact.SerieDocu.Substring(0, 1).Equals("B")) // Boleta
                    {
                        xmlNacional = xmlNacional
                        + "<cbc:TaxAmount currencyID='" + Fact.TipMone + "'>" + Fact.Igv + "</cbc:TaxAmount>"
                        + "<cac:TaxSubtotal>"
                        + "<cbc:TaxableAmount currencyID='" + Fact.TipMone + "'>" + String.Format("{0:0.00}", Fact.SubTotal) + "</cbc:TaxableAmount>"
                        + "<cbc:TaxAmount currencyID='" + Fact.TipMone + "'>" + String.Format("{0:0.00}", Fact.Igv) + "</cbc:TaxAmount>";

                    }

                    xmlNacional = xmlNacional
                        // " <cbc:TaxAmount currencyID='" + Fact.TipMone + "'>0.00</cbc:TaxAmount>"
                     + "<cac:TaxCategory>"
                     + "<cbc:ID schemeAgencyName='United Nations Economic Commission for Europe' schemeID='UN/ECE 5305' schemeName='Tax Category Identifier'>S</cbc:ID>"
                     + "<cac:TaxScheme>";


                    if (Fact.SerieDocu.Substring(0, 1).Equals("F")) // Factura
                    {
                        xmlNacional = xmlNacional
                        + "<cbc:ID schemeAgencyName='PE:SUNAT' schemeID='UN/ECE 5153' schemeName='Codigo de tributos'>9995</cbc:ID>"
                        + "<cbc:Name>EXP</cbc:Name>"
                        + "<cbc:TaxTypeCode>FRE</cbc:TaxTypeCode>";
                    }
                    else if (Fact.SerieDocu.Substring(0, 1).Equals("B")) // Boleta
                    {
                        xmlNacional = xmlNacional
                       + "<cbc:ID schemeAgencyName='PE:SUNAT' schemeID='UN/ECE 5153' schemeName='Codigo de tributos'>1000</cbc:ID>"
                       + "<cbc:Name>IGV</cbc:Name>"
                       + "<cbc:TaxTypeCode>VAT</cbc:TaxTypeCode>";
                    }

                    xmlNacional = xmlNacional
                    + "</cac:TaxScheme>"
                    + "</cac:TaxCategory>"
                    + "</cac:TaxSubtotal>"
                    + "</cac:TaxTotal>";

                    break;

            }

            
            
             switch (TipoDocumento)
            {
                case "B":
                     xmlNacional = xmlNacional
                     + " <cac:LegalMonetaryTotal>"
                     + "<cbc:LineExtensionAmount currencyID='" + Fact.TipMone + "'>" + String.Format("{0:0.00}", Fact.SubTotal)  + "</cbc:LineExtensionAmount>";
              
                     break;
               case "F":
                     xmlNacional = xmlNacional
                     + " <cac:LegalMonetaryTotal>"
                     + "<cbc:LineExtensionAmount currencyID='" + Fact.TipMone + "'>" + String.Format("{0:0.00}", Fact.Total)  + "</cbc:LineExtensionAmount>";
                    break;
                    xmlNacional = xmlNacional
                    +"</cac:LegalMonetaryTotal>";
            }


             // "<!-- Cabecera - Despatch Advice. TIENDA - Seller_ Supplier - Proveedor Remitente - Party. Supplier Party -->"
            switch(TipoDocumento)
            {              
  
                    //+ "<cac:PartyIdentification>"
                    //+ "<cbc:ID schemeAgencyName='PE:SUNAT' schemeID='6'  schemeName='Documento de Identidad' schemeURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo06'>" + Fact.RucDocu_emp + "</cbc:ID>"
                    //+ "</cac:PartyIdentification>";
                case "GR":                    
                        xmlNacional = xmlNacional
                        + "<cac:SellerSupplierParty>"

                        //+ "<cbc:CustomerAssignedAccountID schemeID='${tipDocProveedor}'>${numDocProveedor}</cbc:CustomerAssignedAccountID>"
                        + "<cbc:CustomerAssignedAccountID schemeID='6'>" + Fact.RucDocu_emp + "</cbc:CustomerAssignedAccountID>"
                                                
                        + "<cbc:AdditionalAccountID/>"
                        + "<cac:Party>"
                        + "<cac:PartyName>"
                        + "<cbc:Name>" + Fact.Nombre_Comercial_Emp + "</cbc:Name>"
                        + "</cac:PartyName>"
                        + "<cac:PartyLegalEntity>"
                        + "<cbc:RegistrationName>" + Fact.Razon_Social_Emp + "</cbc:RegistrationName>"
                        + "</cac:PartyLegalEntity>"
                        + "</cac:Party>"
                        + "</cac:SellerSupplierParty>";
                     break;
            }

            switch (TipoDocumento)
            { 
           
                case "ND":
                    xmlNacional = xmlNacional
                    + "<cac:RequestedMonetaryTotal>";
                    break;
            }
            
            switch(TipoDocumento){
                case "B":
                case "F":
                case "NC" :
                    xmlNacional = xmlNacional
                        //(AllowanceTotalAmount) Monto total de descuentos del comprobante 
                    + "<cbc:AllowanceTotalAmount currencyID='" + Fact.TipMone + "'>" + String.Format("{0:0.00}", Fact.Descu) + "</cbc:AllowanceTotalAmount>"

                    //(ChargeTotalAmount) Monto total de otros cargos del comprobante  // falta modificar
                    + "<cbc:ChargeTotalAmount currencyID='" + Fact.TipMone + "'>0.00</cbc:ChargeTotalAmount>"

                    //(PrepaidAmount) Monto total de anticipos del comprobante         // falta modificar
                    + "<cbc:PrepaidAmount currencyID='" + Fact.TipMone + "'>0.00</cbc:PrepaidAmount>"
                    + "<cbc:PayableAmount currencyID='" + Fact.TipMone + "'>" + String.Format("{0:0.00}", Fact.Total) + "</cbc:PayableAmount>";
                    
                    break;
                case "ND":
                    xmlNacional = xmlNacional
                    + "<cbc:LineExtensionAmount currencyID='" + Fact.TipMone + "'>" + String.Format("{0:0.00}", Fact.Total) + "</cbc:LineExtensionAmount>"
                    + "<cbc:TaxExclusiveAmount currencyID='" + Fact.TipMone + "'>0.00</cbc:TaxExclusiveAmount>"
                    + "<cbc:ChargeTotalAmount currencyID='" + Fact.TipMone + "'>0.00</cbc:ChargeTotalAmount>"
                    + "<cbc:PayableAmount currencyID='" + Fact.TipMone + "'>" + String.Format("{0:0.00}", Fact.Total) + "</cbc:PayableAmount>";

                    break;
            }

            //<!-- Cabecera - Datos del Env�o -->
            switch(TipoDocumento){
                case "GR":
                    //<!-- Cabecera - Datos del Env�o -->
                    xmlNacional = xmlNacional
                    + "<cac:Shipment>"
                    + "<cbc:ID>1</cbc:ID>"

                    //<!-- Datos del Envio - Motivo de Traslado -->(Cat_#20)
                    //+ "<cbc:HandlingCode>${motTrasladoDatosEnvio}</cbc:HandlingCode>"
                    + "<cbc:HandlingCode>01</cbc:HandlingCode>"


                    //<!-- Datos del Env�o - Descripcion del motive del traslado --> Information == (OPCIONAL)
                    + "<cbc:Information>traslado de paquetes</cbc:Information>"
                    //<!-- Datos del Env�o - Peso bruto total de la gu�a -->
                    //GrossWeightMeasure
                    //+ "<cbc:GrossWeightMeasure unitCode='${uniMedidaPesoBrutoDatosEnvio}'>${psoBrutoTotalBienesDatosEnvio}</cbc:GrossWeightMeasure>"
                    + "<cbc:GrossWeightMeasure unitCode='KG'>16.500</cbc:GrossWeightMeasure>"
                    //<!-- Datos del Env�o - Numero de bultos o pallets - Enteros-->
	                // #if numBultosDatosEnvio?? && numBultosDatosEnvio != "-" && numBultosDatosEnvio != "" 
                    //+ "<cbc:TotalTransportHandlingUnitQuantity>${numBultosDatosEnvio}</cbc:TotalTransportHandlingUnitQuantity>"
                    
                    // TotalTransportHandlingUnitQuantity ==== OPCIONAL
                    + "<cbc:TotalTransportHandlingUnitQuantity>3</cbc:TotalTransportHandlingUnitQuantity>"
                    
                    //</#if>
                    //<!-- Datos del Env�o - Indicador del transbordo programado -->
                    //+ "<cbc:SplitConsignmentIndicator>${indTransbordoProgDatosEnvio}</cbc:SplitConsignmentIndicator>"
                    + "<cbc:SplitConsignmentIndicator>true</cbc:SplitConsignmentIndicator>"

                    + "<cac:Consignment>"
                    + "<cbc:ID>001</cbc:ID>"
                    + "</cac:Consignment>"
                    //<!-- Datos del Env�o - Per�odo de embarque -->
                    + "<cac:ShipmentStage>"
                        + "<cbc:ID>1</cbc:ID>"
                        //<!-- Datos del Env�o - Embarque - Modalidad del traslado -->
                        //+ "<cbc:TransportModeCode>${modTrasladoDatosEnvio}</cbc:TransportModeCode>"
                        + "<cbc:TransportModeCode>2</cbc:TransportModeCode>"

                        //<!--cbc:TransitDirectionCode>20</cbc:TransitDirectionCode>
                        //+ "<cbc:PreCarriageIndicator>false</cbc:PreCarriageIndicator>"
                        //+ "<cbc:OnCarriageIndicator>false</cbc:OnCarriageIndicator-->"
                        //<!-- Datos del Env�o - Embarque - Fecha Salida -->
                        + "<cac:TransitPeriod>"

                        //+ "<cbc:StartDate>${fecInicioTrasladoDatosEnvio}</cbc:StartDate>"
                        + "<cbc:StartDate>2015-10-23</cbc:StartDate>"
                        
                        + "</cac:TransitPeriod>"

                        //====== TRANSPORTE PUBLICO ======
                        ////<!-- Datos del Env�o - Embarque - Transporte publico-->
                        //+ "<cac:CarrierParty>"
                        //+ "<cac:PartyIdentification>"
                        ////+ "<cbc:ID schemeID='${tipDocTransportista}'>${numDocTransportista}</cbc:ID>"
                        //+ "<cbc:ID schemeID='6'>85442051</cbc:ID>"
                        //+ "</cac:PartyIdentification>"
                        //+ "<cac:PartyName>"
                        ////+ "<cbc:Name>${nomTransportista}</cbc:Name>"
                        //+ "<cbc:Name>JORGE GOMEZ TAVARA</cbc:Name>"
                        //+ "</cac:PartyName>"
                        ////<!-- cac:PartyLegalEntity>
                        ////<cbc:RegistrationName>MARVISUR SAC</cbc:RegistrationName>
                        ////</cac:PartyLegalEntity-->
                        //+ "</cac:CarrierParty>"
                        //====== END TRANSPORTE PUBLICO ======

                        //<!-- Datos del Env�o - Embarque - Medios de Transporte privado-->
                        + "<cac:TransportMeans>"
                        + "<cac:RoadTransport>"
                        //<!-- Datos del Env�o - Embarque - Medios de Transporte - Placa -->
                        //+ "<cbc:LicensePlateID>${numPlacaTransPrivado}</cbc:LicensePlateID>"
                        + "<cbc:LicensePlateID>PGY-0988</cbc:LicensePlateID>"
                        + "</cac:RoadTransport>"
                        + "</cac:TransportMeans>"
                        //<!-- Datos del Conductor  - Embarque - Medios de Transporte privado-->
                        + "<cac:DriverPerson>"
                        //+ "<cbc:ID schemeID='${tipDocIdeConductorTransPrivado}'>${numDocIdeConductorTransPrivado}</cbc:ID>"
                        + "<cbc:ID schemeID='1'>72631059</cbc:ID>"
                        //+ "<cbc:FirstName>${nomConductorTransPrivado}</cbc:FirstName>"
                        + "<cbc:FirstName>SOHO TORRES REYES</cbc:FirstName>"

                        + "</cac:DriverPerson>"
                    + "</cac:ShipmentStage>"
                    //<!-- Datos del Envio - Entrega -->
                    + "<cac:Delivery>"
                    //<!-- Entrega - Direcci�n -->
                    + "<cac:DeliveryAddress>"

                    //+ "<cbc:ID>${ubiLlegada}</cbc:ID>"
                    + "<cbc:ID>120606</cbc:ID>"

                    //+ "<cbc:StreetName>${dirLlegada}</cbc:StreetName>"
                    + "<cbc:StreetName><![CDATA[JR.  MANTARO NRO. 257]]></cbc:StreetName>"

                    + "</cac:DeliveryAddress>"
                    + "</cac:Delivery>"

                    //<!-- Datos del Envio - codigo del contenedor -->
                    + "<cac:TransportHandlingUnit>"
                    + "<cac:TransportEquipment>"

                    //+ "<cbc:ID>${numContenedor}</cbc:ID>"
                    + "<cbc:ID>120606</cbc:ID>"
                    
                    + "</cac:TransportEquipment>"
                    + "</cac:TransportHandlingUnit>"

                    //<!-- Datos del Envio - Direcci�n Origen -->
                    + "<cac:OriginAddress>"
                    //+ "<cbc:ID>${ubiPartida}</cbc:ID>"
                    + "<cbc:ID>150123</cbc:ID>"
                    //+ "<cbc:StreetName>${dirPartida}</cbc:StreetName>"
                    + "<cbc:StreetName><![CDATA[CAR. PANAM SUR KM 25 NO. 25050 NRO. 050 Z.I. CONCHAN]]></cbc:StreetName>"
                    + "</cac:OriginAddress>"

                    //<!-- Datos del Envio - codigo del puerto -->
                    + "<cac:FirstArrivalPortLocation>"
                    //+ "<cbc:ID>${codPuerto}</cbc:ID>"
                    + "<cbc:ID>PAI</cbc:ID>"                    
                    + "</cac:FirstArrivalPortLocation>"

                    + "</cac:Shipment>";
                    break;
            }

            switch (TipoDocumento)
            {
                case "ND":
                    xmlNacional = xmlNacional
                   + "</cac:RequestedMonetaryTotal>";
                    break;
            }


            switch(TipoDocumento){
                case "B":
                case "F":
                    xmlNacional = xmlNacional
                    + "</Invoice>";
                    break;
                case "ND":
                    xmlNacional = xmlNacional
                   + "</DebitNote>";
                    break;
                    
                case "NC":
                    xmlNacional = xmlNacional
                    + "</CreditNote>";
                    break;  
                case "GR":
                    xmlNacional = xmlNacional
                    + "</DespatchAdvice>";
                    break;
            }
            //<!------------------------- Fin Tributos Cabecera ------------------------->

            //---------------------------------------------------------------------------
            // --------------------------------  DETALLE -------------------------------- 
            
            try
            {

                using (var tw = new StreamWriter(path, true))
                {
                    tw.WriteLine(xmlNacional);
                }

                /*
                if (File.Exists(path))
                {
                    // sobrescribe en archivo
                    System.IO.File.WriteAllText(path, xmlNacional);
                }
                else
                {
                    // Crea el archivo.
                    using (FileStream fs = File.Create(path))
                    {
                        System.IO.File.WriteAllText(path, xmlNacional);
                    }
                    using (var tw = new StreamWriter(path, true))
                    {
                        tw.WriteLine(TextBox1.Text);
                    }
                

                }*/
                // Imprime consola.
                // Abrir 
                using (StreamReader sr = File.OpenText(path))
                {
                    string s = "";
                    while ((s = sr.ReadLine()) != null)
                    {
                        Console.WriteLine(s);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }


            // ****************************** DETALLE ****************************************
            String xmlString = System.IO.File.ReadAllText(ConfigurationManager.AppSettings["RutaXml"] + "/" + NombreArchivoXml);
            XmlDocument doc2 = new XmlDocument();
            doc2.PreserveWhitespace = true;

            //CARGAR TEXTO XML EN VARIABLE DOC2
            try
            {
                doc2.LoadXml(xmlString);
            }
            catch (System.IO.FileNotFoundException f)
            {
                Console.Out.WriteLine(f.Message);
            }
            
            String l_xdet = "";

            
            switch (TipoDocumento)
            {
                case "F":
                case "B":
                    l_xdet = "/tns:Invoice";
                    break;
                case "NC":
                    l_xdet = "/tns:CreditNote";
                    break;
                case "ND":
                    l_xdet = "/tns:DebitNote";
                    break;
                case "GR":
                    l_xdet = "/tns:DespatchAdvice";
                    break;
            }

            XmlNamespaceManager ns = new XmlNamespaceManager(doc2.NameTable);
            if (TipoDocumento.Equals("NC")) {
                ns.AddNamespace("tns", "urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2");
            }
            else if (TipoDocumento.Equals("ND"))
            {
                ns.AddNamespace("tns", "urn:oasis:names:specification:ubl:schema:xsd:DebitNote-2");
            }
            else if (TipoDocumento.Equals("GR"))
            {
                ns.AddNamespace("tns", "urn:oasis:names:specification:ubl:schema:xsd:DespatchAdvice-2");
            }
            else
            {
                ns.AddNamespace("tns", "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2");
            }
            ns.PushScope();
            ns.AddNamespace("ext", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");
            ns.PushScope();
            ns.AddNamespace("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
            ns.PushScope();
            ns.AddNamespace("udt", "urn:un:unece:uncefact:data:specification:UnqualifiedDataTypesSchemaModule:2");
            ns.PushScope();
            ns.AddNamespace("qdt", "urn:oasis:names:specification:ubl:schema:xsd:QualifiedDatatypes-2");
            ns.PushScope();
            ns.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            ns.PushScope();
            ns.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
            ns.PushScope();


            percent = 18.0;

            int cont_b = 1;
            cont_txt_item = cont_txt_item - 1;
            while (cont_txt_item >= cont_b)
            {
                if (Fact.Cod_tip_ope.Equals("0200"))
                {
                    igv_item = 0.00;
                    percent = 0.00;
                    ValorUnitario = Math.Round(Convert.ToDouble(ValorVenta_item[cont_b]), 2);
                }
                else
                {
                    Console.Write(texto_Item.Count());

                    //igv_item = Cantidades[cont_b]; - ERROR COMENTADO
                    igv_item = 0.00;

                    ValorUnitario = Math.Round(Convert.ToDouble(ValorVenta_item[cont_b]) * 1.18, 2);
                    ValorUnitario = Convert.ToDouble(ValorVenta_item[cont_b]) * 1.18;
                    Console.Write(Math.Round(Convert.ToDouble(ValorVenta_item[cont_b]), 2));

                    ValorUnitario = Fact.Total - (ValorUnitario * Cantidades[cont_b]);
                }

                
                // ESCRIBIR NUEVOS ELEMENTOS EN XML
                XmlWriter xmlw = doc2.SelectSingleNode(l_xdet, ns).CreateNavigator().AppendChild();

                switch (TipoDocumento)
                {
                    case "F":
                    case "B":
                        xmlw.WriteStartElement("InvoiceLine", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
                        break;
                    case "NC":
                        xmlw.WriteStartElement("CreditNoteLine", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
                        break;
                    case "ND":
                        xmlw.WriteStartElement("DebitNoteLine", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
                        break;
                    case "GR":
                        xmlw.WriteStartElement("DespatchLine", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
                        break;
                }

                xmlw.WriteElementString("ID", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2", cont_b.ToString());

                
                switch (TipoDocumento)
                {
                    case "F":
                    case "B":
                        xmlw.WriteStartElement("InvoicedQuantity", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                        break;
                    case "NC":
                        xmlw.WriteStartElement("CreditedQuantity", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                        break;
                    case "ND":
                        //"<cbc:LineExtensionAmount currencyID='USD'>0.00</cbc:LineExtensionAmount>"
           
                        xmlw.WriteStartElement("DebitedQuantity", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                        break;


                    case "GR":
                        xmlw.WriteStartElement("DeliveredQuantity", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                        break;
                }

                switch (TipoDocumento)
                {
                case "F":
                case "B":
                case "NC":
                case "ND" :
                    xmlw.WriteAttributeString("unitCode", "NIU");
                    //xmlw.WriteAttributeString("unitCodeListID", "UN/ECE rec 20");
                    //xmlw.WriteAttributeString("unitCodeListAgencyName", "United Nations Economic Commission for Europe");
                    xmlw.WriteString(String.Format("{0:0.00}", Cantidades[cont_b]));//23 .- Unidad Mededida Item (Obligatorio)
                    xmlw.WriteEndElement();
                    xmlw.WriteStartElement("LineExtensionAmount", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                    xmlw.WriteAttributeString("currencyID", Fact.TipMone);
                    xmlw.WriteString(String.Format("{0:0.00}",0));
                    xmlw.WriteEndElement();
                    ///////////////////////////  LineExtensionAmount/////////////////////////////////////////////////////
                    //xmlw.WriteStartElement("LineExtensionAmount", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                    //xmlw.WriteAttributeString("currencyID", Fact.TipMone);
                    break;

                case "GR":
                        xmlw.WriteAttributeString("unitCode", "NIU");
                        //xmlw.WriteAttributeString("unitCodeListID", "UN/ECE rec 20");
                        //xmlw.WriteAttributeString("unitCodeListAgencyName", "United Nations Economic Commission for Europe");
                        xmlw.WriteString(String.Format("{0:0.00}", Cantidades[cont_b]));
                        xmlw.WriteEndElement();
                        break;
                    //case "ND":
                    ///////////////////////////  LineExtensionAmount/////////////////////////////////////////////////////
                    //    xmlw.WriteStartElement("LineExtensionAmount", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                    //    xmlw.WriteAttributeString("currencyID", Fact.TipMone);

                    //    break;
                }
                
                switch (TipoDocumento)
                {
                    case "F":
                        xmlw.WriteString(String.Format("{0:0.00}", (Math.Round(Cantidades[cont_b] * Convert.ToDouble(ValorVenta_item[cont_b]), 2))));
                        break;
                    case "B":
                        //   xmlw.WriteString((Math.Round(Convert.ToDouble(ValorVenta_item[cont_b]), 2)).ToString());
                        xmlw.WriteString(String.Format("{0:0.00}", Math.Round((Cantidades[cont_b] * Convert.ToDouble(ValorVenta_item[cont_b]) / 1.18), 2)));
                        //.WriteString(Math.Round(ValorVenta_item(cont_b), 2)) ' Boleta 
                        //  xmlw.WriteString(String.Format("{0:0.00}", Math.Round((Cantidades[cont_b] * Convert.ToDouble(ValorVenta_item[cont_b])/1.18),2)));
                        MessageBox.Show((Cantidades[cont_b] * Convert.ToDouble(ValorVenta_item[cont_b]) / 1.18).ToString());

                        // Console.Write(Math.Round((Cantidades[cont_b] * Convert.ToDouble(ValorVenta_item[cont_b])) / 1.18, 2));
                        break;
                    //case "ND":
                    //    if (Fact.SerieDocu.Substring(0, 1).Equals("F"))
                    //    {
                    //        //xmlw.WriteString(String.Format("{0:0.00}", 0));
                    //        xmlw.WriteString("Hola");
                    //    }
                    //    //else if (Fact.TipoDocumentoAnulacionNC.Equals("03"))
                    //    else if (Fact.SerieDocu.Substring(0, 1).Equals("B"))
                    //    {
                    //        xmlw.WriteString(String.Format("{0:0.00}", 0));
                    //    }
                    //    break;

                    case "NC":
                        //if (Fact.TipoDocumentoAnulacionNC.Equals("01"))
                        if (Fact.SerieDocu.Substring(0, 1).Equals("F")) {
                            xmlw.WriteString(String.Format("{0:0.00}", (Math.Round(Cantidades[cont_b] * Convert.ToDouble(ValorVenta_item[cont_b]), 2))));
                        }
                        //else if (Fact.TipoDocumentoAnulacionNC.Equals("03"))
                        else if (Fact.SerieDocu.Substring(0, 1).Equals("B")) {
                            xmlw.WriteString(String.Format("{0:0.00}", (Math.Round((Cantidades[cont_b] * Convert.ToDouble(ValorVenta_item[cont_b])) / 1.18, 2))));
                        }
                        break;
                }
              //  xmlw.WriteEndElement();
                /////////////////////////  LineExtensionAmount end///////////////////////////////////////////////////// // cambio
                switch(TipoDocumento){
                    case "F":
                    case "B":
                    case "NC":
                        //////////////////////// Billing Reference LINE /////////////////////////////////////////////////////
                        //' BillingReference
                        xmlw.WriteStartElement("BillingReference","urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
                        //' BillingReferenceLine
                        xmlw.WriteStartElement("BillingReferenceLine","urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
                        xmlw.WriteElementString("ID", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2", cont_b.ToString());
                        // BillingReference  end 
                        xmlw.WriteEndElement();
                        // BillingReferenceLine end
                        xmlw.WriteEndElement();
                        //////////////////////// END Billing Reference LINE /////////////////////////////////////////////////////

                        /////////////////////////////////////'PricingReference //////////////////////////////
                        xmlw.WriteStartElement("PricingReference", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
                        xmlw.WriteStartElement("AlternativeConditionPrice", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
                        xmlw.WriteStartElement("PriceAmount", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                        xmlw.WriteAttributeString("currencyID", Fact.TipMone);
                    break;
                    case "ND":
                        xmlw.WriteStartElement("PricingReference", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
                        xmlw.WriteStartElement("AlternativeConditionPrice", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
                        xmlw.WriteStartElement("PriceAmount", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                        xmlw.WriteAttributeString("currencyID", Fact.TipMone);

                    break;
                    
                }

                switch (TipoDocumento)
                {
                    case "F":
                        xmlw.WriteString(String.Format("{0:0.00}", ValorUnitario));
                        break;
                    case "B":
                        if (Fact.CantidadBolsa > 0 && (CD_item[cont_b] == "BOL"))
                        {
                            xmlw.WriteString((Convert.ToDouble(ValorVenta_item[cont_b]) + Fact.PrecioUnitBolsa).ToString());
                            
                        }
                        else
                        {
                           xmlw.WriteString(String.Format("{0:0.00}", (Math.Round(Convert.ToDouble(ValorVenta_item[cont_b]), 10))));
                        }
                        
                     //   Console.Write(((Convert.ToDouble(ValorVenta_item[cont_b])) + Fact.ImporteBolsa).ToString());
                        MessageBox.Show(Math.Round(Convert.ToDouble(ValorVenta_item[cont_b]), 10).ToString());
                        break;
                    case "NC":
                    case "ND":

                        //if (Fact.TipoDocumentoAnulacionNC.Equals("01"))
                        if (Fact.SerieDocu.Substring(0, 1).Equals("F")) {
                          //  xmlw.WriteString(String.Format("{0:0.00}", Item_Clasificacion[cont_txt_item]));
                          xmlw.WriteString(String.Format("{0:0.00}", ValorUnitario)); //29 Precio de Venta Unitario Item
                        }
                        //else if (Fact.TipoDocumentoAnulacionNC.Equals("03"))
                        else if (Fact.SerieDocu.Substring(0, 1).Equals("B"))
                        {
                            xmlw.WriteString(String.Format("{0:0.00}", (Convert.ToDouble(ValorVenta_item[cont_b]))));
                        }
                        break;
                }

                switch (TipoDocumento)
                {
                    case "F":
                    case "B":
                    case "NC":
                    case "ND":
                        xmlw.WriteEndElement();
                        //'PriceTypeCode
                        xmlw.WriteStartElement("PriceTypeCode", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                        xmlw.WriteAttributeString("listAgencyName", "PE:SUNAT");
                        //'listName
                        xmlw.WriteAttributeString("listName", "Tipo de Precio");
                        //'listName
                        xmlw.WriteAttributeString("listURI", "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo16");
                        xmlw.WriteString("01");
                        xmlw.WriteEndElement();
                        xmlw.WriteEndElement();
                        xmlw.WriteEndElement();
                        break;
               
                }
                /////////////////////////////////////'PricingReference//////////////////////////////


                ///////////////////////////////////'TaxTotal
                switch (TipoDocumento)
                {
                    case "F":
                    case "B":
                    case "NC":
                    case "ND":
                        xmlw.WriteStartElement("TaxTotal", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
                        xmlw.WriteStartElement("TaxAmount", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                        xmlw.WriteAttributeString("currencyID", Fact.TipMone);
                        break;
                }

                switch (TipoDocumento)
                {
                    case "F":
                        xmlw.WriteString(String.Format("{0:0.00}", "0"));//Cambio
                        break;
                    case "B":
                        // xmlw.WriteString(String.Format("{0:0.00}",Math.Round((((Math.Round((Convert.ToDouble(ValorVenta_item[cont_b]) / 1.18), 2) * Cantidades[cont_b]) * 1.18) - (Math.Round((Convert.ToDouble(ValorVenta_item[cont_b]) / 1.18), 2) * Cantidades[cont_b])), 2)));
                        //((Cantidades[cont_b] * Convert.ToDouble(ValorVenta_item[cont_b])) / 1.18) *0.18
                        if (Fact.CantidadBolsa > 0 && (CD_item[cont_b] == "BOL"))
                        {
                            xmlw.WriteString(String.Format("{0:0.00}", (Math.Round((Cantidades[cont_b] * Fact.ImporteTotalBolsa) - (((Cantidades[cont_b] * Convert.ToDouble(ValorVenta_item[cont_b])) / 1.18)), 2))));
                        }
                        else
                        {
                            xmlw.WriteString(String.Format("{0:0.00}", (Math.Round((((Cantidades[cont_b] * Convert.ToDouble(ValorVenta_item[cont_b])) / 1.18) * 0.18), 2))));
                        }
                        //  Console.Write((Math.Round((((Cantidades[cont_b] * Convert.ToDouble(ValorVenta_item[cont_b])) / 1.18) * 0.18), 2)));
                        //  MessageBox.Show(Math.Round((Cantidades[cont_b] * Convert.ToDouble(ValorVenta_item[cont_b]) * 0.18), 2));
                        break;
                    case "ND":
                        //if (Fact.TipoDocumentoAnulacionNC.Equals("01"))
                        if (Fact.SerieDocu.Substring(0, 1).Equals("F"))
                        {
                            xmlw.WriteString(String.Format("{0:0.00}", (Math.Round(Cantidades[cont_b] * Convert.ToDouble(ValorVenta_item[cont_b]), 2))));
                        }
                        //else if (Fact.TipoDocumentoAnulacionNC.Equals("03"))
                        else if (Fact.SerieDocu.Substring(0, 1).Equals("B"))
                        {
                            xmlw.WriteString(String.Format("{0:0.00}", (Math.Round((Cantidades[cont_b] * Convert.ToDouble(ValorVenta_item[cont_b])) / 1.18, 2))));
                        }
                        break;
                }

                switch (TipoDocumento)
                {
                    case "F":
                    case "B":
                    case "NC":
                    case "ND":
                        xmlw.WriteEndElement();
                        //'TaxSubtotal

                        //Inicio Impuesto selectivo Consumo//////
                        xmlw.WriteStartElement("TaxSubtotal", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
                        xmlw.WriteStartElement("TaxableAmount", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                        xmlw.WriteAttributeString("currencyID", Fact.TipMone);
                        break;
                }

                switch (TipoDocumento)
                {
                    case "F":
                        xmlw.WriteString(String.Format("{0:0.00}", (Math.Round(Cantidades[cont_b] * Convert.ToDouble(ValorVenta_item[cont_b]), 2))));
                        break;
                    case "B":
                        xmlw.WriteString(String.Format("{0:0.00}", (Math.Round((Cantidades[cont_b] * Convert.ToDouble(ValorVenta_item[cont_b])) / 1.18, 2))));
                        //.WriteString(Math.Round((Cantidades(cont_b) * ValorVenta_item(cont_b)) / 1.18, 2))
                        break;
                    case "NC":
                        //if (Fact.TipoDocumentoAnulacionNC.Equals("01"))
                        if (Fact.SerieDocu.Substring(0, 1).Equals("F"))
                        {
                            xmlw.WriteString(String.Format("{0:0.00}", (Math.Round(Cantidades[cont_b] * Convert.ToDouble(ValorVenta_item[cont_b]), 2))));
                        }
                        //else if (Fact.TipoDocumentoAnulacionNC.Equals("03"))
                        else if (Fact.SerieDocu.Substring(0, 1).Equals("B")) {
                            xmlw.WriteString(String.Format("{0:0.00}", (Math.Round((Cantidades[cont_b] * Convert.ToDouble(ValorVenta_item[cont_b])) / 1.18, 2))));
                        }
                        break;
                    case "ND":
                        //if (Fact.TipoDocumentoAnulacionNC.Equals("01"))
                        if (Fact.SerieDocu.Substring(0, 1).Equals("F"))
                        {
                            xmlw.WriteString(String.Format("{0:0.00}", (Math.Round(Cantidades[cont_b] * Convert.ToDouble(ValorVenta_item[cont_b]), 2))));
                        }
                        //else if (Fact.TipoDocumentoAnulacionNC.Equals("03"))
                        else if (Fact.SerieDocu.Substring(0, 1).Equals("B"))
                        {
                            xmlw.WriteString(String.Format("{0:0.00}", (Math.Round((Cantidades[cont_b] * Convert.ToDouble(ValorVenta_item[cont_b])) / 1.18, 2))));
                        }
                        break;
                }

                switch (TipoDocumento)
                {
                    case "F":
                    case "B":
                    case "NC":
                    case "ND":
                        xmlw.WriteEndElement();
                        //'TaxAmount
                        xmlw.WriteStartElement("TaxAmount", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                        xmlw.WriteAttributeString("currencyID", Fact.TipMone);
                    //  xmlw.WriteString(Math.Round(igv_item,2).ToString()); // igv 
                        break;
                }

                //String.Format("{0:00}", igv_item)


                switch (TipoDocumento)
                {
                    case "F":
                        xmlw.WriteString(String.Format("{0:0.00}", igv_item));
                        break;
                    case "B":
                        // xmlw.WriteString(String.Format("{0:0.00}", Math.Round((((Math.Round((Convert.ToDouble(ValorVenta_item[cont_b]) / 1.18), 2) * Cantidades[cont_b]) * 1.18) - (Math.Round((Convert.ToDouble(ValorVenta_item[cont_b]) / 1.18), 2) * Cantidades[cont_b])), 2)));
                        //.WriteString(Math.Round((Cantidades(cont_b) * ValorVenta_item(cont_b)) / 1.18, 2))
                        // xmlw.WriteString(String.Format("{0:0.00}", (Math.Round((((Cantidades[cont_b] * Convert.ToDouble(ValorVenta_item[cont_b])) / 1.18) * 0.18), 2))));
                        if (Fact.CantidadBolsa > 0 && (CD_item[cont_b] == "BOL"))                     
                        {
                            xmlw.WriteString(String.Format("{0:0.00}", (((Cantidades[cont_b] * Convert.ToDouble(ValorVenta_item[cont_b])) - Math.Round(((Cantidades[cont_b] * Convert.ToDouble(ValorVenta_item[cont_b])) / 1.18), 2)))));
                            //xmlw.WriteString(String.Format("{0:0.00}", (Math.Round(((((Cantidades[cont_b] * Convert.ToDouble(ValorVenta_item[cont_b])) / 1.18))), 2))));
                            // Console.Write(String.Format("{0:0.00}",(Math.Round(((Cantidades[cont_b] * Fact.ImporteTotalBolsa) - (((Cantidades[cont_b] * Convert.ToDouble(ValorVenta_item[cont_b])) / 1.18) * 0.18)), 2))));
                             //MessageBox.Show(Math.Round(((Cantidades[cont_b] * Fact.ImporteTotalBolsa) - (((Cantidades[cont_b] * Convert.ToDouble(ValorVenta_item[cont_b])) / 1.18))), 2).ToString());
                            MessageBox.Show(String.Format("{0:0.00}", (((Cantidades[cont_b] * Convert.ToDouble(ValorVenta_item[cont_b])) - Math.Round(((Cantidades[cont_b] * Convert.ToDouble(ValorVenta_item[cont_b])) / 1.18), 2)))));
                        }
                        else
                        {
                            xmlw.WriteString(String.Format("{0:0.00}", (Math.Round((((Cantidades[cont_b] * Convert.ToDouble(ValorVenta_item[cont_b])) / 1.18) * 0.18), 2))));
                        }
                        break;
                    case "ND":
                        if (Fact.SerieDocu.Substring(0, 1).Equals("F")) {
                            xmlw.WriteString(String.Format("{0:0.00}", igv_item));
                        }
                        //else if (Fact.TipoDocumentoAnulacionNC.Equals("03"))
                        else if (Fact.SerieDocu.Substring(0, 1).Equals("B")) {
                           // xmlw.WriteString(String.Format("{0:0.00}", (Math.Round((Cantidades[cont_b] * Convert.ToDouble(ValorVenta_item[cont_b])) / 1.18, 2))));
                        }
                        break;
                        break;
                }

                switch (TipoDocumento)
                {
                    case "F":
                    case "B":
                    case "NC":
                    case "ND":
                        //// xmlw.WriteString(String.Format("{0:0.00}",Math.Round((((Math.Round((Convert.ToDouble(ValorVenta_item[cont_b]) / 1.18), 2) * Cantidades[cont_b]) * 1.18) - (Math.Round((Convert.ToDouble(ValorVenta_item[cont_b]) / 1.18), 2) * Cantidades[cont_b])), 2)));
                        xmlw.WriteEndElement();
                        //'TaxAmount
                        xmlw.WriteStartElement("TaxCategory", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");

                        //*** id categoria / 14/01/2020
                        xmlw.WriteStartElement("ID", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                        xmlw.WriteAttributeString("schemeAgencyName", "United Nations Economic Commission for Europe");
                        xmlw.WriteAttributeString("schemeID", "UN/ECE 5305");
                        xmlw.WriteAttributeString("schemeName", "Tax Category Identifier");
                        /////////////////////////////////// 14-01-2020
                        //Categoría  del  Impuesto 
                        //xmlw.WriteString(ValidarCampos.Obtener_CatTributo(Fact.CodTribu));
                        xmlw.WriteString(Fact.CatImpuesto);
                        /////////////////////////////////// end
                        xmlw.WriteEndElement();
                        //*** end _ id categoria 14/01/2020

                        xmlw.WriteStartElement("Percent", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                        xmlw.WriteString(String.Format("{0:0.00}", percent));
                        xmlw.WriteEndElement();
                        //'TaxExemptionReasonCode
                        xmlw.WriteStartElement("TaxExemptionReasonCode", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                        xmlw.WriteAttributeString("listAgencyName", "PE:SUNAT");

                        // xmlw.WriteAttributeString("listName", Fact.NomAfecta);
                        xmlw.WriteAttributeString("listName", "IGV Impuesto General a las Ventas");
                        xmlw.WriteAttributeString("listURI", "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo07");
                        xmlw.WriteString(Fact.codafecta.ToString());
                        xmlw.WriteEndElement();
                        //'TaxExemptionReasonCode
                        //'TaxScheme
                        xmlw.WriteStartElement("TaxScheme", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
                        //'ID
                        xmlw.WriteStartElement("ID", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                        xmlw.WriteAttributeString("schemeID", "UN/ECE 5153");
                        xmlw.WriteAttributeString("schemeName", "Codigo de tributos");
                        xmlw.WriteAttributeString("schemeAgencyName", "PE:SUNAT");
                        xmlw.WriteAttributeString("schemeURI", "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo07");
                        xmlw.WriteString(Fact.CodTribu);
                        xmlw.WriteEndElement();
                        //'ID end 
                        //'Name
                        xmlw.WriteStartElement("Name", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                        xmlw.WriteString(Fact.NomTributo);
                        xmlw.WriteEndElement();
                        //'Nameend 
                        //'TaxTypeCode
                        xmlw.WriteStartElement("TaxTypeCode", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                        xmlw.WriteString(Fact.TipeCodeAtribu);
                        xmlw.WriteEndElement();
                        //'TaxTypeCode 
                        xmlw.WriteEndElement();
                        // 'TaxScheme end
                        xmlw.WriteEndElement();
                        //'TaxAmount end
                        xmlw.WriteEndElement();
                        //'TaxSubtotal
                        break;
                }



                //xmlw.WriteStartElement("TaxSubtotal", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
                //xmlw.WriteStartElement("TaxableAmount", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                //xmlw.WriteAttributeString("currencyID", Fact.TipMone);

                //switch (TipoDocumento)
                //{
                //    case "F":
                //        xmlw.WriteString(String.Format("{0:0.00}", (Math.Round(Cantidades[cont_b] * Convert.ToDouble(ValorVenta_item[cont_b]), 2))));
                //        break;
                //    case "B":
                //        xmlw.WriteString(String.Format("{0:0.00}", (Math.Round((Cantidades[cont_b] * Convert.ToDouble(ValorVenta_item[cont_b])) / 1.18, 2))));
                //        //.WriteString(Math.Round((Cantidades(cont_b) * ValorVenta_item(cont_b)) / 1.18, 2))
                //        Console.Write(Math.Round((Cantidades[cont_b] * Convert.ToDouble(ValorVenta_item[cont_b])) / 1.18, 2));

                //        break;
                //    case "NC":
                //        if (Fact.TipoDocumentoAnulacionNC.Equals("01"))
                //        {
                //            xmlw.WriteString((Math.Round(Cantidades[cont_b] * Convert.ToDouble(ValorVenta_item[cont_b]), 2)).ToString());
                //        }
                //        else if (Fact.TipoDocumentoAnulacionNC.Equals("03"))
                //        {
                //            xmlw.WriteString((Math.Round((Cantidades[cont_b] * Convert.ToDouble(ValorVenta_item[cont_b])) / 1.18, 2)).ToString());
                //        }
                //        break;

                //}


                //xmlw.WriteEndElement();
                ////'TaxAmount
                //xmlw.WriteStartElement("TaxAmount", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                //xmlw.WriteAttributeString("currencyID", Fact.TipMone);
                ////  xmlw.WriteString(Math.Round(igv_item,2).ToString()); // igv 

                ////String.Format("{0:00}", igv_item)

                //xmlw.WriteString(String.Format("{0:0.00}", igv_item));

                //// xmlw.WriteString(String.Format("{0:0.00}",Math.Round((((Math.Round((Convert.ToDouble(ValorVenta_item[cont_b]) / 1.18), 2) * Cantidades[cont_b]) * 1.18) - (Math.Round((Convert.ToDouble(ValorVenta_item[cont_b]) / 1.18), 2) * Cantidades[cont_b])), 2)));
                //xmlw.WriteEndElement();
                ////'TaxAmount
                //xmlw.WriteStartElement("TaxCategory", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
                //// agregado 14-01-2020
                //xmlw.WriteStartElement("ID", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                //xmlw.WriteAttributeString("schemeAgencyName", "United Nations Economic Commission for Europe");
                //xmlw.WriteAttributeString("schemeID", "UN/ECE 5305");
                //xmlw.WriteAttributeString("schemeName", "Tax Category Identifier");
                ////xmlw.WriteString(ValidarCampos.Obtener_CatTributo(Fact.CodTribu));
                ////  xmlw.WriteString(Fact.CatImpuesto);
                //xmlw.WriteString("S");
                //xmlw.WriteEndElement();
                //// END - agregado 14-01-2020 

                //xmlw.WriteStartElement("Percent", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");

                //switch (TipoDocumento)
                //{
                //    case "F":
                //        xmlw.WriteString(String.Format("{0:0.00}", percent));
                //        break;
                //    case "B":
                //        xmlw.WriteString(String.Format("{0:0.00}", "0"));
                //        break;
                //}




                ////urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2
                //xmlw.WriteEndElement();

                //xmlw.WriteStartElement("TierRange", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                //xmlw.WriteString("0");
                //xmlw.WriteEndElement();
                ////'TaxExemptionReasonCode

                ////'TaxExemptionReasonCode
                ////'TaxScheme
                //xmlw.WriteStartElement("TaxScheme", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
                ////'ID
                //xmlw.WriteStartElement("ID", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                //xmlw.WriteAttributeString("schemeAgencyName", "PE:SUNAT");
                //xmlw.WriteAttributeString("schemeID", "UN/ECE 5153");
                //xmlw.WriteAttributeString("schemeName", "Codigo de tributos");
                //xmlw.WriteString("2000");
                //xmlw.WriteEndElement();
                ////'ID end 
                ////'Name
                //xmlw.WriteStartElement("Name", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                //xmlw.WriteString("ISC");
                //xmlw.WriteEndElement();
                ////'Nameend 
                ////'TaxTypeCode
                //xmlw.WriteStartElement("TaxTypeCode", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                //xmlw.WriteString("EXC");
                //xmlw.WriteEndElement();
                ////'TaxTypeCode 
                //xmlw.WriteEndElement();
                //// 'TaxScheme end
                //xmlw.WriteEndElement();
                ////'TaxAmount end
                //xmlw.WriteEndElement();
                //'TaxSubtotal
                //  xmlw.WriteEndElement();

              
                //Fin Impuesto
                //////////////////////////////////

                //switch (TipoDocumento)
                //{
                //    case "B":


                //        if (Fact.CantidadBolsa > 0 && (CD_item[cont_b] == "BOL"))
                //        {
                //            xmlw.WriteStartElement("TaxSubtotal", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
                //            xmlw.WriteStartElement("TaxAmount", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                //            xmlw.WriteAttributeString("currencyID", Fact.TipMone);
                //            xmlw.WriteString(Fact.ImporteBolsa.ToString());
                //            xmlw.WriteEndElement();
                //            xmlw.WriteStartElement("BaseUnitMeasure", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
                //            xmlw.WriteAttributeString("unitCode", "NIU");
                //            xmlw.WriteString(Fact.CantidadBolsa.ToString());
                //            xmlw.WriteEndElement();
                //            xmlw.WriteStartElement("TaxCategory", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
                //            xmlw.WriteStartElement("PerUnitAmount", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
                //            xmlw.WriteAttributeString("currencyID", Fact.TipMone);
                //            xmlw.WriteString(Fact.PrecioUnitBolsa.ToString());
                //            xmlw.WriteEndElement();
                //            xmlw.WriteStartElement("TaxScheme", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");

                //            xmlw.WriteStartElement("ID", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                //            xmlw.WriteAttributeString("schemeAgencyName", "PE:SUNAT");
                //            xmlw.WriteAttributeString("schemeURI", "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo05");
                //            xmlw.WriteAttributeString("schemeName", "Codigo de tributos");
                //            xmlw.WriteString("7152");
                //            xmlw.WriteEndElement();
                //            //'ID end 
                //            //'Name
                //            xmlw.WriteStartElement("Name", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                //            xmlw.WriteString("ICBPER");
                //            xmlw.WriteEndElement();
                //            //'Nameend 
                //            //'TaxTypeCode
                //            xmlw.WriteStartElement("TaxTypeCode", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                //            xmlw.WriteString("OTH");
                //            xmlw.WriteEndElement();
                //            //'TaxTypeCode 
                //            xmlw.WriteEndElement();
                //            // 'TaxScheme end
                //            xmlw.WriteEndElement();
                //            //'TaxAmount end
                //            xmlw.WriteEndElement();
                //            //'TaxSubtotal end
                //        }
                //        break;
                //}

                switch (TipoDocumento) {
                    case "GR":
                        xmlw.WriteStartElement("OrderLineReference", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
                        xmlw.WriteStartElement("LineID", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                        xmlw.WriteCData(cont_b.ToString());
                        //'LineID end
                        xmlw.WriteEndElement();
                        //'OrderLineReference end
                        xmlw.WriteEndElement();
                        break;
                }

                xmlw.WriteEndElement();//CAMBIO
                // 'TaxTotal
                
                switch (TipoDocumento)
                {
                    case "F":
                    case "B":
                    case "NC":
                        //////////////////////////' Item /////////////////////////////////
                        xmlw.WriteStartElement("Item", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
                        //'Description
                        xmlw.WriteStartElement("Description", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                        xmlw.WriteCData(String.Format("{0:0.00}", Dsc_Item[cont_b]));
                        xmlw.WriteEndElement();
                        //'Description end 
                        //'SellersItemIdentification
                        xmlw.WriteStartElement("SellersItemIdentification", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
                        //'ID
                        xmlw.WriteStartElement("ID", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                        xmlw.WriteString(CD_item[cont_b]);
                        xmlw.WriteEndElement();
                        //'ID end 
                        xmlw.WriteEndElement();
                        //'SellersItemIdentification end
                        //'CommodityClassification
                        xmlw.WriteStartElement("CommodityClassification", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
                        //'ItemClassificationCode
                        xmlw.WriteStartElement("ItemClassificationCode", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                        xmlw.WriteAttributeString("listID", "UNSPSC");
                        xmlw.WriteAttributeString("listAgencyName", "GS1 US");
                        xmlw.WriteAttributeString("listName", "Item Classification");
                        xmlw.WriteString(Item_Clasificacion[cont_b]);
                        xmlw.WriteEndElement();
                        //'ItemClassificationCode end 
                        xmlw.WriteEndElement();
                        //'CommodityClassification end
                        xmlw.WriteEndElement();
                        //////////////////////////////////Item end////////////////////////////

                        ///////////////////////////////////Price////////////////////////////////////
                        xmlw.WriteStartElement("Price", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
                        //PriceAmount
                        xmlw.WriteStartElement("PriceAmount", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                        xmlw.WriteAttributeString("currencyID", Fact.TipMone);
                        break;
                    case "ND" :
                        //////////////////////////' Item /////////////////////////////////
                        xmlw.WriteStartElement("Item", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
                        //'Description
                        xmlw.WriteStartElement("Description", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                        xmlw.WriteCData(String.Format("{0:0.00}", Dsc_Item[cont_b])); // Descripcion del Item 26 Note debito
                        xmlw.WriteEndElement();
                        //'Description end 
                        //'SellersItemIdentification
                        xmlw.WriteStartElement("SellersItemIdentification", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
                        //'ID
                        xmlw.WriteStartElement("ID", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                        xmlw.WriteString(CD_item[cont_b]);
                        xmlw.WriteEndElement();
                        //'ID end 
                        xmlw.WriteEndElement();
                        //xmlw.WriteEndElement();

                         xmlw.WriteStartElement("CommodityClassification", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
                        //'ItemClassificationCode
                        xmlw.WriteStartElement("ItemClassificationCode", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                        xmlw.WriteAttributeString("listID", "UNSPSC");
                        xmlw.WriteAttributeString("listAgencyName", "GS1 US");
                        xmlw.WriteAttributeString("listName", "Item Classification");
                        xmlw.WriteString(Item_Clasificacion[cont_b]);
                        xmlw.WriteEndElement();
                        //'ItemClassificationCode end 
                        xmlw.WriteEndElement();
                        xmlw.WriteEndElement();

                        ///////////////////////////////////Price////////////////////////////////////
                        xmlw.WriteStartElement("Price", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
                        //PriceAmount
                        xmlw.WriteStartElement("PriceAmount", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                        xmlw.WriteAttributeString("currencyID", Fact.TipMone); //Valor Unitario por ItEM
           
                        break;
                    case "GR":
                        xmlw.WriteStartElement("Item", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
                        xmlw.WriteStartElement("Name", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                        xmlw.WriteCData(String.Format("{0:0.00}", Dsc_Item[cont_b]));
                        //'Name end
                        xmlw.WriteEndElement();
                        xmlw.WriteStartElement("SellersItemIdentification", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
                        xmlw.WriteStartElement("ID", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                        xmlw.WriteString(CD_item[cont_b]);
                        //'ID end
                        xmlw.WriteEndElement();
                        //'SellersItemIdenti end
                        xmlw.WriteEndElement();
                        //'Item end
                        xmlw.WriteEndElement();
                        break;
                }


                //.WriteString(Math.Round(ValorVenta_item(cont_b) / 1.18, 2)) 'Boleta

                switch (TipoDocumento)
                {
                    case "F":
                        xmlw.WriteString(String.Format("{0:0.00}", ValorVenta_item[cont_b]));// factura
                        break;
                    case "B":
                        xmlw.WriteString(String.Format("{0:0.00}", (Math.Round((Convert.ToDouble(ValorVenta_item[cont_b])) / 1.18, 2))));
                        break;

                    case "ND":
                       if (Fact.SerieDocu.Substring(0, 1).Equals("F"))
                        {
                            xmlw.WriteString(String.Format("{0:0.00}", (Math.Round(Cantidades[cont_b] * Convert.ToDouble(ValorVenta_item[cont_b]), 2))));
                           // xmlw.WriteString(String.Format("{0:0.00}", Item_Clasificacion[cont_txt_item]));
                        }
                        //else if (Fact.TipoDocumentoAnulacionNC.Equals("03"))
                        else if (Fact.SerieDocu.Substring(0, 1).Equals("B")) {
                            xmlw.WriteString(String.Format("{0:0.00}", (Math.Round((Cantidades[cont_b] * Convert.ToDouble(ValorVenta_item[cont_b])) / 1.18, 2))));
                        }
                        break;
                       

                }

                switch (TipoDocumento)
                {
                    case "F":
                    case "B":
                    case "NC" :
                        xmlw.WriteEndElement();
                        //PriceAmount end
                        xmlw.WriteEndElement();
                        //////////////////////////////////////Price end//////////////////////////////////////                                         
                        xmlw.WriteEndElement();
                        break;
                    case "ND":
                          xmlw.WriteEndElement();
                        //PriceAmount end
                        xmlw.WriteEndElement();
                        break;
     
                }

                xmlw.Close();
                /*    
                foreach (XmlNode xn in xmlw)
                {
                string firstName = xn["FirstName"].InnerText;
                string lastName = xn["LastName"].InnerText;
                Console.WriteLine("Name: {0} {1}", firstName, lastName);
                }*/
                cont_b += 1;

            }

            doc2.Save(ConfigurationManager.AppSettings["RutaXml"]+ "/" + NombreArchivoXml);
            Tools  Util = new Tools();
            
          //  Util.ZipXMl(ConfigurationManager.AppSettings["RutaXml"] + "/" + NombreArchivoXml, ConfigurationManager.AppSettings["DestinoPathZip"] + "/" + NombreArchivo + ".zip");


            Util.CreateZipFile(ConfigurationManager.AppSettings["RutaXml"] + "/" + NombreArchivoXml, ConfigurationManager.AppSettings["DestinoPathZip"] + "/" + NombreArchivo + ".zip");

            // ==== END DETALLE ====

            MessageBox.Show("Se generó XML satisfactoriamente!");

            
            // ====== AGREGAR FIRMA DIGITAL A XML ======
            string fn_firma = "firmademo-20102089635.pfx";
            //string l_xml = @"C:\Users\Romain\Desktop\SUNAT\test csharp\20381235051-01-FF11-01.xml";
            string rt_xml = System.Configuration.ConfigurationSettings.AppSettings["RutaXml"].ToString();
            string rt_cer = System.Configuration.ConfigurationSettings.AppSettings["RutaFirma"].ToString();

            string l_xml = rt_xml + "/" + NombreArchivoXml;
            string l_certificado = rt_cer + "/" + fn_firma;
            string l_pwd = "clave2020";
            string l_xpath;

            X509Certificate2 l_cert = new X509Certificate2(l_certificado, l_pwd);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            xmlDoc.Load(l_xml);

            SignedXml signedXml = new SignedXml(xmlDoc);

            signedXml.SigningKey = l_cert.PrivateKey;
            KeyInfo KeyInfo = new KeyInfo();

            Reference Reference = new Reference();
            Reference.Uri = "";

            Reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            signedXml.AddReference(Reference);

            X509Chain X509Chain = new X509Chain();
            X509Chain.Build(l_cert);

            X509ChainElement local_element = X509Chain.ChainElements[0];
            KeyInfoX509Data x509Data = new KeyInfoX509Data(local_element.Certificate);
            string subjectName = local_element.Certificate.Subject;

            x509Data.AddSubjectName(subjectName);
            KeyInfo.AddClause(x509Data);

            signedXml.KeyInfo = KeyInfo;
            signedXml.ComputeSignature();

            XmlElement signature = signedXml.GetXml();
            signature.Prefix = "ds";
            signedXml.ComputeSignature();

            foreach (XmlNode loop_node in signature.SelectNodes("descendant-or-self::*[namespace-uri()='http://www.w3.org/2000/09/xmldsig#']"))
            {
                if (loop_node.LocalName == "Signature")
                {
                    XmlAttribute newAttribute = xmlDoc.CreateAttribute("Id");
                    newAttribute.Value = "SignatureSP";
                    loop_node.Attributes.Append(newAttribute);
                }

            }

            XmlNamespaceManager nsMgr = new XmlNamespaceManager(xmlDoc.NameTable);
            nsMgr.AddNamespace("sac", "urn:sunat:names:specification:ubl:peru:schema:xsd:SunatAggregateComponents-1");
            nsMgr.AddNamespace("ccts", "urn:un:unece:uncefact:documentation:2");
            nsMgr.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");

            if (l_xml.Contains("-01-") || l_xml.Contains("-03-")) //factura - boleta
            {
                nsMgr.AddNamespace("tns", "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2");
                l_xpath = "/tns:Invoice/ext:UBLExtensions/ext:UBLExtension/ext:ExtensionContent";
                //l_xpath = "/tns:Invoice/ext:UBLExtensions/ext:UBLExtension[2]/ext:ExtensionContent";
            }
            else if (l_xml.Contains("-07-")) //nota de crédito
            {
                nsMgr.AddNamespace("tns", "urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2");
                l_xpath = "/tns:CreditNote/ext:UBLExtensions/ext:UBLExtension/ext:ExtensionContent";
            }
            else if (l_xml.Contains("-08-"))//nota de débito
            {
                nsMgr.AddNamespace("tns", "urn:oasis:names:specification:ubl:schema:xsd:DebitNote-2");
                l_xpath = "/tns:DebitNote/ext:UBLExtensions/ext:UBLExtension/ext:ExtensionContent";
            }
            else if (l_xml.Contains("-09-"))// Guia Remision
            {
                nsMgr.AddNamespace("tns", "urn:oasis:names:specification:ubl:schema:xsd:DespatchAdvice-2");
                l_xpath = "/tns:DespatchAdvice/ext:UBLExtensions/ext:UBLExtension/ext:ExtensionContent";
            }
            else // communicacion de baja
            {
                nsMgr.AddNamespace("tns", "urn:sunat:names:specification:ubl:peru:schema:xsd:VoidedDocuments-1");
                l_xpath = "/tns:VoidedDocuments/ext:UBLExtensions/ext:UBLExtension/ext:ExtensionContent";
            }

            nsMgr.AddNamespace("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
            nsMgr.AddNamespace("udt", "urn:un:unece:uncefact:data:specification:UnqualifiedDataTypesSchemaModule:2");
            nsMgr.AddNamespace("ext", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");
            nsMgr.AddNamespace("qdt", "urn:oasis:names:specification:ubl:schema:xsd:QualifiedDatatypes-2");
            nsMgr.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            nsMgr.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");


            /*
                xmlDoc.SelectSingleNode(l_xpath,nsMgr).AppendChild(xmlDoc.ImportNode(signature, true));
            */
            XmlNode fnode = xmlDoc.SelectSingleNode(l_xpath, nsMgr);

            if (fnode != null)
            {
                fnode.AppendChild(xmlDoc.ImportNode(signature, true));
            }

            xmlDoc.Save(l_xml);

            //check signature (verificacion signature)
            XmlNodeList nodeList = xmlDoc.GetElementsByTagName("ds:Signature");
            if (nodeList.Count <= 0)
            {
                MessageBox.Show("Verification failed: No Signature was found in the document.(Falló verificacion)");
            }
            else if (nodeList.Count >= 2)
            {
                MessageBox.Show("Verification failed: More that one signature was found for the document.");
            }
            else
            {
                signedXml.LoadXml((XmlElement)nodeList[0]);
                if (signedXml.CheckSignature())
                {
                    MessageBox.Show("Se añadio Firma a XML satisfactoriamente");
                    //CreateZip(System.Configuration.ConfigurationSettings.AppSettings["RutaXml"].ToString() + "/" + NombreArchivoXml, NombreArchivoXml);
                    //MessageBox.Show("Se comprimio el archivo XML!");
                }
                else
                {
                    MessageBox.Show("Falló signature!");
                }
            }
        }

        //public static void CreateZip(string pathXML, string namexml)
        //{
        //    // ruta para guardar ZIP (firmado)
        //    string path = System.Configuration.ConfigurationSettings.AppSettings["RutaFirmado"].ToString();

        //    // nombre del zip
        //    string[] a = namexml.Split('.');

        //    path += "/" + a[0] + ".zip";

        //    // Obtiene text del xml
        //    string readText = File.ReadAllText(pathXML);

        //    byte[] byteArray = ASCIIEncoding.ASCII.GetBytes(readText);
        //    string encodedText = Convert.ToBase64String(byteArray);

        //    FileStream destFile = File.Create(path);

        //    byte[] buffer = Encoding.UTF8.GetBytes(encodedText);
        //    MemoryStream memoryStream = new MemoryStream();

        //    using (System.IO.Compression.GZipStream gZipStream = new System.IO.Compression.GZipStream(destFile, System.IO.Compression.CompressionMode.Compress, true))
        //    {
        //        gZipStream.Write(buffer, 0, buffer.Length);
        //    }
        //}

    }
}