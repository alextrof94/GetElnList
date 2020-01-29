using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CryptoPro.Sharpei;
using CryptoPro.Sharpei.Xml;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.Net;
using System.IO;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;

namespace GetElnList
{
    public partial class Form1 : Form
    {
        /* debug settings */
        static int DEBUG_STEP = 6;
        static bool externalFileSign = false;
        static bool externalFileEncryption = false;
        static bool TEST = false;
        static string UrlTest = "https://docs-test.fss.ru/ws-insurer-crypto-v11/FileOperationsLnPort?xsd=2";
        static string Url = "https://docs.fss.ru/ws-insurer-crypto-v11/FileOperationsLnPort?xsd=2";
        /* END debug settings */

        // Настройки
        static bool DELETE_BUF_FILES = true;
        static string tempFolder = Path.GetTempPath(); // Выбор папки для хранения промежуточных файлов, сейчас выбирается временная папка пользователя, в которую всегда есть доступ у приложений и не надо париться с правами

        // Различные хранилища
        List<DataSet> dataSets = new List<DataSet>();
        List<DataSet> dataSetsHasErrors = new List<DataSet>();
        List<ResultData> resultDatas = new List<ResultData>();
        X509Certificate2 certOur, certFss;

        public Form1()
        {
            InitializeComponent();

            if (String.IsNullOrEmpty(Properties.Settings.Default.certOurSN)
                    || String.IsNullOrEmpty(Properties.Settings.Default.certFssSN)) // Если один из сертификатов отсутствует
                getSertificates(); // получаем сертификаты с компа
            else
            {
                certOur = certificateFindBySerialNumber(Properties.Settings.Default.certOurSN); // Загружаем сертификаты по серийнику
                certFss = certificateFindBySerialNumber(Properties.Settings.Default.certFssSN);
                if (certOur == null || certFss == null) // Если не удалось загрузить сертификат по серийнику
                    getSertificates(); // Выбираем с компа
            }
        }

        /// <summary>
        /// Поиск сертификатов по серийнику
        /// </summary>
        /// <param name="sn">Серийник</param>
        /// <returns>Сертификат</returns>
        public X509Certificate2 certificateFindBySerialNumber(string sn)
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser); // Создаем объект хранилища
            store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadOnly); // Открываем его
            X509Certificate2Collection certificates = store.Certificates; // Получаем список доступных сертификатов
            foreach (X509Certificate2 cert in certificates) // Бежим по сертификатам
                if (cert.SerialNumber == sn && cert.NotAfter > DateTime.Now && cert.NotBefore < DateTime.Now) // Если серийники совпадают и сертификат действителен
                    return cert; // Вернем его
            return null; // Еслиничего не нашли - вернем нулл
        }

        /// <summary>
        /// Выбираем сертификаты для работы
        /// </summary>
        public void getSertificates()
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser); // Создаем объект хранилища
            X509Certificate2Collection scollection;
            // Получаем сертификат ОТПРАВИТЕЛЯ (НАШ)
            do
            {
                store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadOnly); // Открываем хранилище
                scollection = X509Certificate2UI.SelectFromCollection(store.Certificates, "Выберите сертификат ОТПРАВИТЕЛЯ (НАШ)", "Выберите сертификат отправителя", X509SelectionFlag.SingleSelection); // Выбор
            } while (scollection.Count == 0); // Если сертификат не был выбран - повторяем
            certOur = scollection[0]; // Записываем сертификат

            scollection = null;
            // Получаем сертификат ПОЛУЧАТЕЛЯ (ФСС)
            do
            {
                store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadOnly);
                scollection = X509Certificate2UI.SelectFromCollection(store.Certificates, "Выберите сертификат ПОЛУЧАТЕЛЯ (ФСС)", "Выберите сертификат получателя", X509SelectionFlag.SingleSelection);
            } while (scollection.Count == 0);
            certFss = scollection[0];
            
            // Дальше пишем серийники и сохраняем настройки
            Properties.Settings.Default.certOurSN = certOur.SerialNumber;
            Properties.Settings.Default.certFssSN = certFss.SerialNumber;
            Properties.Settings.Default.Save();
        }

        private void buAdd_Click(object sender, EventArgs e)
        {
            if (tbLnCode.Text.Length > 0 && tbRegNum.Text.Length > 0 && tbSnils.Text.Length > 0) // Если все поля заполнены
            {
                // Создаем объект
                DataSet ds = new DataSet(tbRegNum.Text, tbLnCode.Text, tbSnils.Text);
                // Если объекта нет в коллекции - добавляем
                if (!dataSets.Any(x => x.Equals(ds)))
                    dataSets.Add(ds);
                // Стираем поле снилс
                tbSnils.Text = "";
                // Перерисовываем элементы списка
                lb.Items.Clear();
                foreach (DataSet d in dataSets)
                    lb.Items.Add(d);
            }
        }

        private void buRemove_Click(object sender, EventArgs e)
        {
            // Если объект списка выбран - удаляем его
            if (lb.SelectedItem != null)
                dataSets.RemoveAll(x => x == (DataSet)lb.SelectedItem);

            // Перерисовываем элементы списка
            lb.Items.Clear();
            foreach (DataSet d in dataSets)
                lb.Items.Add(d);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            getSertificates(); // Выбираем сертификаты
        }
    
        /// <summary>
        /// Создает заполненный XML-файл в папке tempFolder из ресурса Source1
        /// </summary>
        /// <param name="ds">DataSet объект, который надо ввести в файл</param>
        /// <returns>Путь созданного файла</returns>
        private string createXml(DataSet ds)
        {
            if (!Directory.Exists(tempFolder)) // Если нет директории
            {
                // Спрашиваем "хотим ли создать ее?"
                if (MessageBox.Show("GetElnList", "Директория \"" + tempFolder + "\" отсутствует. Создать?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    Directory.CreateDirectory(tempFolder); // Создаем
                else
                    throw new Exception("Шаг 1. Требуемая папка отсутствует."); // Либо выбиваем исключение
            }

            string filename1 = tempFolder + "FSS_" + DateTime.Now.Ticks.ToString() + ".xml"; // Создаем временный файл
            
            // Создаем xml из ресурса
            XmlDocument document = new XmlDocument();
            document.LoadXml(Properties.Resources.Source1.Replace("{OGRN}", ds.regNum));

            // Заполняем его
            if (TEST) 
            {
                // Если выставлен флаг теста
                document.DocumentElement.GetElementsByTagName("fil:regNum").Item(0).InnerText = ds.regNum;
                document.DocumentElement.GetElementsByTagName("fil:lnCode").Item(0).InnerText = "306767144098";
                document.DocumentElement.GetElementsByTagName("fil:snils").Item(0).InnerText = "00000000001";
            }
            else
            {
                // Флаг теста не выставлен, обращаемся к ФСС напрямую
                document.DocumentElement.GetElementsByTagName("fil:regNum").Item(0).InnerText = ds.regNum;
                document.DocumentElement.GetElementsByTagName("fil:lnCode").Item(0).InnerText = ds.lnCode;
                document.DocumentElement.GetElementsByTagName("fil:snils").Item(0).InnerText = ds.snils;
            }

            // Сохраняем документ в файле
            document.Save(filename1);

            // Отображаем файл если требуется дебаг 
            if (DEBUG_STEP < 1)
            {
                foView view = new foView(); // Создаем форму просмотра
                view.Text = filename1; // Говорим что выводить в шапке
                view.xmlFileName = filename1; // Что потребуется загрузить
                view.Show(); // Показываем
            }

            return filename1; // Возвращаем имя созданного файла
        }
       
        /// <summary>
        /// Подписывает XML файл по пути
        /// </summary>
        /// <param name="filename">Путь к файлу для подписи</param>
        /// <returns>Путь подписанного файла</returns>
        public string signXml(string filename)
        {
            if (!File.Exists(filename))
                throw new Exception("Шаг 2. Файл не найден!\r\nПуть:" + filename);

            string filename2 = filename + ".signed.xml";
            
            // Открываем файл для подписи
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true; // Всякие пробелы обязательно оставляем
            xmlDoc.Load(filename);

            // Создаем особый XML для подписи взятый с сайта http://cryptopro.ru/blog/2012/05/16/podpis-soobshchenii-soap-dlya-smev-s-ispolzovaniem-kriptopro-net
            SmevSignedXml signedXml = new SmevSignedXml(xmlDoc);
            signedXml.SigningKey = certOur.PrivateKey; // Подписываем нашим закрытым ключом

            // Дальше идет магия, особо мне непонятная
            Reference reference = new Reference(); // Создаем ссылку для файла
            reference.Uri = "#REGNO_1603774817"; // Ее УРИ

            reference.DigestMethod = CPSignedXml.XmlDsigGost3411UrlObsolete; // Указываем устаревший метод описания. Обязательно этот устаревший

            XmlDsigExcC14NTransform c14 = new XmlDsigExcC14NTransform(); // Создаем метод канонизации
            reference.AddTransform(c14); // Подключаем ссылку

            signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigExcC14NTransformUrl; // Подключаем метод канонизации к нашему файлу

            signedXml.SignedInfo.SignatureMethod = CPSignedXml.XmlDsigGost3410UrlObsolete; // Опять указываем устаревший метод подписи

            signedXml.AddReference(reference); // Добавляем ссылку

            KeyInfo keyInfo = new KeyInfo(); // Создаем инфо о ключе
            keyInfo.AddClause(new KeyInfoX509Data(certOur)); // О нашем ключе
            signedXml.KeyInfo = keyInfo; // Указываем ее

            signedXml.ComputeSignature(); // Вычисляем подпись

            XmlElement xmlDigitalSignature = signedXml.GetXml(); // Получаем xml

            // Перетасовываем всю инфу в файл для сохранения
            xmlDoc.GetElementsByTagName("ds:Signature")[0].PrependChild(xmlDoc.ImportNode(xmlDigitalSignature.GetElementsByTagName("SignatureValue")[0], true));
            xmlDoc.GetElementsByTagName("ds:Signature")[0].PrependChild(xmlDoc.ImportNode(xmlDigitalSignature.GetElementsByTagName("SignedInfo")[0], true));
            xmlDoc.GetElementsByTagName("wsse:BinarySecurityToken")[0].InnerText = Convert.ToBase64String(certOur.RawData);

            xmlDoc.Save(filename2); // Сохраняем подписанный документ в файле

            // Отображаем файл если требуется для дебага
            if (DEBUG_STEP < 2)
            {
                foView view = new foView();
                view.Text = filename2;
                view.xmlFileName = filename2;
                view.Show();
            }
            return filename2;
        }

        /// <summary>
        /// Шифрует XML файл по пути
        /// </summary>
        /// <param name="filename">Путь файла</param>
        /// <returns>Путь зашифрованного файла</returns>
        public string encryptXml(string filename)
        {
            if (externalFileSign) // Если выставлен флаг подписи внешними средствами - грузим его.
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.InitialDirectory = Path.GetTempPath();
                ofd.Filter = "*.xml|*.xml";
                string f = null;
                while (f == null)
                {
                    ofd.ShowDialog();
                    f = ofd.FileName;
                }
                filename = f;
            }
            if (!File.Exists(filename))
                throw new Exception("Шаг 3. Файл не найден!\r\nПуть:" + filename);

            string filename3 = filename + ".encrypted.xml";

            // Открываем файл для шифрования
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            xmlDoc.Load(filename);

            // Шифруем по методу создания ключа обмена
            EncryptedXml eXml = new EncryptedXml();
            EncryptedData edElement = eXml.Encrypt(xmlDoc.DocumentElement, certFss);
            edElement.Type = CPEncryptedXml.XmlEncGost28147Url;
            XmlElement xe = edElement.GetXml();

            XmlDocument xmlDocEnc = new XmlDocument();
            xmlDocEnc.LoadXml(Properties.Resources.Source2);

            // "Правильная" замена сертификата на наш, и вставка прочих данных
            xmlDocEnc.DocumentElement.GetElementsByTagName("ds:X509Certificate").Item(0).InnerText =
                Convert.ToBase64String(certOur.RawData);
            xmlDocEnc.DocumentElement.GetElementsByTagName("xenc:CipherValue").Item(0).InnerText =
                xe.GetElementsByTagName("CipherValue").Item(0).InnerText;
            xmlDocEnc.DocumentElement.GetElementsByTagName("xenc:CipherValue").Item(1).InnerText =
                xe.GetElementsByTagName("CipherValue").Item(1).InnerText;

            // Сохраняем зашифрованный документ в файле
            xmlDocEnc.Save(filename3);

            // Отображаем файл если требуется для дебага
            if (DEBUG_STEP < 3)
            {
                foView view = new foView();
                view.Text = filename3;
                view.xmlFileName = filename3;
                view.Show();
            }

            return filename3;
        }

        /// <summary>
        /// Отсылает файл по пути
        /// </summary>
        /// <param name="filename">Путь файла</param>
        /// <returns>Путь файла-ответа</returns>
        private string sendXml(string filename)
        {
            if (externalFileEncryption) // Если задан флаг внешнего шифрования - грузим файл.
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.InitialDirectory = Path.GetTempPath();
                ofd.Filter = "*.xml|*.xml";
                string f = null;
                while (f == null)
                {
                    ofd.ShowDialog();
                    f = ofd.FileName;
                }
                filename = f;
            }

            if (!File.Exists(filename))
                throw new Exception("Шаг 4. Файл не найден!\r\nПуть:" + filename);

            string filename4 = filename + ".getted.xml";

            try
            {
                // Создаем и обрабатываем обычный http запрос метод POST
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create((TEST) ? UrlTest : Url);
                request.ContentType = "text/xml;charset=utf-8";
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.79 Safari/537.36";
                request.Method = "POST";
                string postData = File.ReadAllText(filename);

                // Пишем файл прямо в тело запроса
                request.GetRequestStream().Write(Encoding.UTF8.GetBytes(postData), 0, postData.Length);

                // Получаем ответ. Тут может вывалиться исключение при отстутствии соединения с серверами ФСС
                WebResponse response = request.GetResponse();

                // Получаем тело ответа
                XmlDocument getted = new XmlDocument();
                getted.Load(response.GetResponseStream());

                // Сохраняем
                getted.Save(filename4);

                // Отображаем файл если требуется для дебага
                if (DEBUG_STEP < 4)
                {
                    foView view = new foView();
                    view.Text = filename4;
                    view.xmlFileName = filename4;
                    view.Show();
                }
            } 
            catch (Exception ex)
            {
                MessageBox.Show("GetElnList", "Возникла ошибка в функции sendXml:\r\n" + ex.Message);
            }
            return filename4;
        }

        /// <summary>
        /// Получает ключ для расшифровки на основе зашифрованных данных (из примеров криптопро)
        /// </summary>
        /// <param name="exml"></param>
        /// <param name="encryptedData"></param>
        /// <returns>Алгоритм для расшифровки</returns>
        private SymmetricAlgorithm GetDecryptionKey(EncryptedXml exml, EncryptedData encryptedData)
        {
            IEnumerator encryptedKeyEnumerator = encryptedData.KeyInfo.GetEnumerator();
            // Проходим по всем KeyInfo
            while (encryptedKeyEnumerator.MoveNext())
            {
                // Пропускам все что неизвестно.
                KeyInfoEncryptedKey current = encryptedKeyEnumerator.Current as KeyInfoEncryptedKey;
                if (current == null) continue;
                // До первого EncryptedKey
                EncryptedKey encryptedKey = current.EncryptedKey;
                if (encryptedKey == null)
                    continue;
                KeyInfo keyinfo = encryptedKey.KeyInfo;
                // Проходим по всем KeyInfo зашифрования ключа.
                IEnumerator srcKeyEnumerator = keyinfo.GetEnumerator();
                while (srcKeyEnumerator.MoveNext())
                {
                    // Пропускам все что неизвестно.
                    KeyInfoX509Data keyInfoCert = srcKeyEnumerator.Current
                        as KeyInfoX509Data;
                    if (keyInfoCert == null)
                        continue;
                    AsymmetricAlgorithm alg = certOur.PrivateKey; // Приватный ключ, открытый ключ которого мы отправляли при шифровании запроса
                    Gost3410 myKey = alg as Gost3410; // Преобразования
                    if (myKey == null)
                        continue;
                    // Получаем и возвращаем ключ для расшифровки
                    return CPEncryptedXml.DecryptKeyClass(encryptedKey.CipherData.CipherValue, myKey, encryptedData.EncryptionMethod.KeyAlgorithm);
                }
            }
            return null;
        }

        /// <summary>
        /// Расшифровывает ответ ФСС по пути
        /// </summary>
        /// <param name="filename">Путь к зашифрованному файлу</param>
        /// <returns>Путь к расшифрованному файлу</returns>
        public string decryptResponse(string filename)
        {
            if (!File.Exists(filename))
                throw new Exception("Шаг 5. Файл не найден!\r\nПуть:" + filename);

            string filename5 = filename + ".decrypted.xml";

            // Создаем объект XmlDocument.
            XmlDocument xmlDoc = new XmlDocument();

            // Загружаем XML файл в объект XmlDocument.
            xmlDoc.PreserveWhitespace = true;
            xmlDoc.Load(filename);

            // Ищем все зашифрованные данные.
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
            nsmgr.AddNamespace("xenc", "http://www.w3.org/2001/04/xmlenc#");
            XmlNodeList list = xmlDoc.SelectNodes("//xenc:EncryptedData", nsmgr);

            // Создаем объект EncryptedXml.
            EncryptedXml exml = new EncryptedXml(xmlDoc);

            if (list != null)
            {
                // Для всех зашифрованных данных.
                foreach (XmlNode node in list)
                {
                    XmlElement element = node as XmlElement;
                    EncryptedData encryptedData = new EncryptedData();
                    encryptedData.LoadXml(element);

                    // Находим подходящий ключ для расшифрования.                    
                    SymmetricAlgorithm decryptionKey = GetDecryptionKey(exml, encryptedData);
                    if (decryptionKey == null)
                        throw new Exception("Ключ для расшифрования сообщения не найден");

                    // И на нем расшифровываем данные.
                    byte[] decryptedData = exml.DecryptData(encryptedData, decryptionKey);
                    exml.ReplaceData(element, decryptedData);
                }
            }

            xmlDoc.Save(filename5);


            // Отображаем файл если требуется для дебага
            if (DEBUG_STEP < 5)
            {
                foView view = new foView();
                view.Text = filename5;
                view.xmlFileName = filename5;
                view.Show();
            }

            return filename5;
        }

        /// <summary>
        /// Получает заполненный объект из расшифрованного ответа ФСС (просто парсит XML)
        /// </summary>
        /// <param name="filename">Путь к расшифрованному ответу ФСС</param>
        /// <returns>Объект</returns>
        public ResultData getResultDataFromXml(string filename)
        {
            ResultData res = new ResultData();
            if (!File.Exists(filename))
                throw new Exception("Шаг 6. Файл не найден!\r\nПуть:" + filename);

            // Создаем объект XmlDocument.
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filename);

            res.STATUS = xmlDoc.DocumentElement.GetElementsByTagName("ns1:STATUS").Item(0).InnerText;
            if (res.STATUS == "1")
            {
                XmlElement row = (XmlElement)xmlDoc.DocumentElement.GetElementsByTagName("ns1:ROW").Item(0);
                res.BIRTHDAY = row.GetElementsByTagName("ns1:BIRTHDAY").Item(0).InnerText;
                res.BOZ_FLAG = row.GetElementsByTagName("ns1:BOZ_FLAG").Item(0).InnerText;
                res.DATE1 = row.GetElementsByTagName("ns1:DATE1").Item(0).InnerText;
                res.DATE2 = row.GetElementsByTagName("ns1:DATE2").Item(0).InnerText;
                res.DT1_LN = row.GetElementsByTagName("ns1:DT1_LN").Item(0).InnerText;
                res.DT2_LN = row.GetElementsByTagName("ns1:DT2_LN").Item(0).InnerText;
                res.DUPLICATE_FLAG = row.GetElementsByTagName("ns1:DUPLICATE_FLAG").Item(0).InnerText;
                res.EMPL_FLAG = row.GetElementsByTagName("ns1:EMPL_FLAG").Item(0).InnerText;
                res.FORM1_DT = row.GetElementsByTagName("ns1:FORM1_DT").Item(0).InnerText;
                res.GENDER = row.GetElementsByTagName("ns1:GENDER").Item(0).InnerText;
                res.HOSPITAL_DT1 = row.GetElementsByTagName("ns1:HOSPITAL_DT1").Item(0).InnerText;
                res.HOSPITAL_DT2 = row.GetElementsByTagName("ns1:HOSPITAL_DT2").Item(0).InnerText;
                res.INSUR_MM = row.GetElementsByTagName("ns1:INSUR_MM").Item(0).InnerText;
                res.INSUR_YY = row.GetElementsByTagName("ns1:INSUR_YY").Item(0).InnerText;
                res.LN_CODE = row.GetElementsByTagName("ns1:LN_CODE").Item(0).InnerText;
                res.LN_DATE = row.GetElementsByTagName("ns1:LN_DATE").Item(0).InnerText;
                res.LN_HASH = row.GetElementsByTagName("ns1:LN_HASH").Item(0).InnerText;
                res.LN_STATE = row.GetElementsByTagName("ns1:LN_STATE").Item(0).InnerText;
                res.LPU_ADDRESS = row.GetElementsByTagName("ns1:LPU_ADDRESS").Item(0).InnerText;
                res.LPU_EMPLOYER = row.GetElementsByTagName("ns1:LPU_EMPLOYER").Item(0).InnerText;
                res.LPU_EMPL_FLAG = row.GetElementsByTagName("ns1:LPU_EMPL_FLAG").Item(0).InnerText;
                res.LPU_NAME = row.GetElementsByTagName("ns1:LPU_NAME").Item(0).InnerText;
                res.LPU_OGRN = row.GetElementsByTagName("ns1:LPU_OGRN").Item(0).InnerText;
                res.MSE_DT1 = row.GetElementsByTagName("ns1:MSE_DT1").Item(0).InnerText;
                res.MSE_DT2 = row.GetElementsByTagName("ns1:MSE_DT2").Item(0).InnerText;
                res.MSE_DT3 = row.GetElementsByTagName("ns1:MSE_DT3").Item(0).InnerText;
                res.MSE_INVALID_GROUP = row.GetElementsByTagName("ns1:MSE_INVALID_GROUP").Item(0).InnerText;
                res.NAME = row.GetElementsByTagName("ns1:NAME").Item(0).InnerText;
                res.NOT_INSUR_MM = row.GetElementsByTagName("ns1:NOT_INSUR_MM").Item(0).InnerText;
                res.NOT_INSUR_YY = row.GetElementsByTagName("ns1:NOT_INSUR_YY").Item(0).InnerText;
                res.PATRONIMIC = row.GetElementsByTagName("ns1:PATRONIMIC").Item(0).InnerText;
                res.PREGN12W_FLAG = row.GetElementsByTagName("ns1:PREGN12W_FLAG").Item(0).InnerText;
                res.PRIMARY_FLAG = row.GetElementsByTagName("ns1:PRIMARY_FLAG").Item(0).InnerText;
                res.REASON1 = row.GetElementsByTagName("ns1:REASON1").Item(0).InnerText;
                res.RETURN_DATE_EMPL = row.GetElementsByTagName("ns1:RETURN_DATE_EMPL").Item(0).InnerText;
                res.SERV1_AGE = row.GetElementsByTagName("ns1:SERV1_AGE").Item(0).InnerText;
                res.SERV1_FIO = row.GetElementsByTagName("ns1:SERV1_FIO").Item(0).InnerText;
                res.SERV1_MM = row.GetElementsByTagName("ns1:SERV1_MM").Item(0).InnerText;
                res.SERV1_RELATION_CODE = row.GetElementsByTagName("ns1:SERV1_RELATION_CODE").Item(0).InnerText;
                res.SERV2_AGE = row.GetElementsByTagName("ns1:SERV2_AGE").Item(0).InnerText;
                res.SERV2_MM = row.GetElementsByTagName("ns1:SERV2_MM").Item(0).InnerText;
                res.SNILS = row.GetElementsByTagName("ns1:SNILS").Item(0).InnerText;
                res.SURNAME = row.GetElementsByTagName("ns1:SURNAME").Item(0).InnerText;

                res.treatPeriods = new List<TreatFullPeriod>();
                XmlNodeList xnlTreatPeriods = row.GetElementsByTagName("ns1:TREAT_PERIODS");
                TreatFullPeriod treatFullPeriod = null;
                for (int i = 0; i < xnlTreatPeriods.Count; i++)
                {
                    treatFullPeriod = new TreatFullPeriod();
                    treatFullPeriod.treatPeriods = new List<TreatPeriod>();

                    XmlElement xeTreatFullPeriod = (XmlElement)xnlTreatPeriods.Item(i);
                    XmlNodeList xnlTreatFullPeriods = xeTreatFullPeriod.GetElementsByTagName("ns1:TREAT_FULL_PERIOD");

                    TreatPeriod treatPeriod = null;
                    for (int j = 0; j < xnlTreatFullPeriods.Count; j++)
                    {
                        treatPeriod = new TreatPeriod();
                        XmlElement xeTreatPeriod = (XmlElement)xnlTreatFullPeriods.Item(i);
                        treatPeriod.TREAT_DOCTOR = xeTreatFullPeriod.GetElementsByTagName("ns1:TREAT_DOCTOR").Item(0).InnerText;
                        treatPeriod.TREAT_DOCTOR_ROLE = xeTreatFullPeriod.GetElementsByTagName("ns1:TREAT_DOCTOR_ROLE").Item(0).InnerText;
                        treatPeriod.TREAT_DT1 = xeTreatFullPeriod.GetElementsByTagName("ns1:TREAT_DT1").Item(0).InnerText;
                        treatPeriod.TREAT_DT2 = xeTreatFullPeriod.GetElementsByTagName("ns1:TREAT_DT2").Item(0).InnerText;
                    }
                    treatFullPeriod.treatPeriods.Add(treatPeriod);
                }
                res.treatPeriods.Add(treatFullPeriod);


                res.lnResult = new List<LnResult>();
                XmlNodeList xnlLnResult = row.GetElementsByTagName("ns1:LN_RESULT");
                LnResult lnResult = null;
                for (int i = 0; i < xnlLnResult.Count; i++)
                {
                    lnResult = new LnResult();
                    XmlElement xeLnResult = (XmlElement)xnlLnResult.Item(i);
                    lnResult.OTHER_STATE_DT = xeLnResult.GetElementsByTagName("ns1:OTHER_STATE_DT").Item(0).InnerText;
                    lnResult.RETURN_DATE_LPU = xeLnResult.GetElementsByTagName("ns1:RETURN_DATE_LPU").Item(0).InnerText;
                }
                res.lnResult.Add(lnResult);
            }

            return res;
        }

        private void buGet_Click(object sender, EventArgs e)
        {
            // если список пуст - ничего не делай
            if (dataSets.Count == 0)
                return;
            try
            {
                for (int i = dataSets.Count - 1; i >= 0; i--) // Проходимся по списку в обратном порядке, чтобы можно было без проблем удалять строки
                {
                    DataSet ds = dataSets[i];
                    string filename1, filename2, filename3, filename4, filename5;

                    // Создаем xml
                    filename1 = createXml(ds);
                    // Подписываем
                    filename2 = signXml(filename1);
                    // Шифруем
                    filename3 = encryptXml(filename2);
                    // Отправляем и получаем ответ
                    filename4 = sendXml(filename3);
                    // Расшифровываем ответ
                    filename5 = decryptResponse(filename4);
                    // Получаем объект
                    ResultData rd = getResultDataFromXml(filename5);
                    if (rd.STATUS == "1") // Если объект по датасету получен верно
                    {
                        // Засовываем результат в лист результатов
                        resultDatas.Add(rd);
                    }
                    else // Не удалось получить объект по датасету
                    {
                        // Засовываем датасет в лист ошибочных датасетов для отображения ошибки
                        dataSetsHasErrors.Add(ds);
                    }

                    // удаляем датасет из листа на проверку
                    dataSets.Remove(ds);

                    // удаляем временные файлы, если требуется
                    if (DELETE_BUF_FILES)
                    {
                        if (File.Exists(filename1))
                            File.Delete(filename1);
                        if (File.Exists(filename2))
                            File.Delete(filename2);
                        if (File.Exists(filename3))
                            File.Delete(filename3);
                        if (File.Exists(filename4))
                            File.Delete(filename4);
                        if (File.Exists(filename5))
                            File.Delete(filename5);
                    }
                }

                // перерисовываем элементы списка
                lb.Items.Clear();
                foreach (DataSet d in dataSets)
                    lb.Items.Add(d);

                // тут можно продолжить работать с resultDatas ==========================================================
                foreach (ResultData rd in resultDatas)
                {
                    // do something
                }

                // вывод ошибочных датасетов
                if (dataSetsHasErrors.Count > 0)
                {
                    string errorMsg = "Возникли ошибки при получении " + dataSetsHasErrors.Count + " датасетов:\r\n";
                    foreach (DataSet ds in dataSetsHasErrors)
                        errorMsg += ds.ToString() + "\r\n";
                    MessageBox.Show(errorMsg, "GetElnList", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.Message, "GetElnList", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
