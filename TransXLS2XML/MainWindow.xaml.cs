using System;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TransXLS2XML
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private Properties.Settings propertiesOrder = new Properties.Settings();
        static private string[] propertiesOrder;

        private Properties.Settings propertiesOfDECLAR = new Properties.Settings();
        private string pathToPropertiesOfDECLAR = "C:\\SSG\\PROJECTs\\Translate-XLS-2-XML\\config.ini";

        private DialogWnd dialogWnd;
        private ConvertorPreparedInfo convertorPrepared;


        private Dictionary<EnumPeriodOfKod, string> dictionaryPeriodOfKods;
        private Dictionary<EnumNumberOfSchemaReport, string> dictionaryDocumentType;

        private string xlsFileName;

        private PreparedSettings4ConvertationStruct _preparedSettings4Convertation;

        //EnumPeriodOfKod

        public MainWindow()
        {
            InitializeComponent();

            dictionaryPeriodOfKods = new Dictionary<EnumPeriodOfKod, string> { [EnumPeriodOfKod.month] = "месяц", [EnumPeriodOfKod.quartal] = "квартал", [EnumPeriodOfKod.semiyear] = "пол года", [EnumPeriodOfKod.nine_months] = "9 месяцев", [EnumPeriodOfKod.year] = "год" };
            dictionaryDocumentType = new Dictionary<EnumNumberOfSchemaReport, string> { [EnumNumberOfSchemaReport.J0147105] = "J0147105", [EnumNumberOfSchemaReport.J1312002] = "J1312002" };




            var hashtable = new Hashtable();

            convertorPrepared = new ConvertorPreparedInfo(pathToPropertiesOfDECLAR);

            var propertyContext = Properties.Settings.Default;

            INIFiles iniFiles = new INIFiles(pathToPropertiesOfDECLAR);

            convertorPrepared.setTypesOfElements(propertiesOrder);


            propertiesOrder = convertorPrepared.getFullKeysNames();

            convertorPrepared.loadProperiesFromINIfile();

            string sDOCNUMBER = convertorPrepared.DOCNUMBER;

            foreach (string s in propertiesOrder)
            {
                string sKey = s.Substring(s.IndexOf('.') + 1);
                string sSection = s.Substring(0, s.IndexOf('.'));
                string sValue;

                if (iniFiles.KeyExists( sSection, s))
                {
                    sValue = iniFiles.ReadINI( sSection, s);
                    hashtable.Add( sKey, sValue);
                }
            }

            foreach(var elT in dictionaryDocumentType)
            {
                cmbDocumentType.Items.Add(elT.Value);
            }
            cmbDocumentType.SelectedItem = dictionaryDocumentType[EnumNumberOfSchemaReport.J0147105];
            foreach (var el in dictionaryPeriodOfKods)
            {
                cbPERIOD_TYPE.Items.Add(el.Value);
            }
            cbPERIOD_TYPE.SelectedItem = dictionaryPeriodOfKods[EnumPeriodOfKod.month];

            tfTIN.Text = (string)hashtable["TIN"];
            tfC_DOC.Text = (string) hashtable["C_DOC"];
            tfC_DOC_SUB.Text = (string) hashtable["C_DOC_SUB"];
            tfPERIOD_MONTH.Text = (string) hashtable["PERIOD_MONTH"];
            tfPERIOD_YEAR.Text = (string) hashtable["PERIOD_YEAR"];
            tfD_FILL.Text = (string) hashtable["D_FILL"];
            tfHSTI.Text = (string) hashtable["HSTI"];
            tfHTIN.Text = (string) hashtable["HTIN"];
            tfHNAME.Text = (string) hashtable["HNAME"];
            tfHBOS.Text = (string) hashtable["HBOS"];
            tfHFILL.Text = (string) hashtable["HFILL"];
            tfC_DOC_CNT.Text = (string)hashtable["C_DOC_CNT"];
            tfC_DOC_STAN.Text = (string)hashtable["C_DOC_STAN"];
            tfC_DOC_VER.Text = (string)hashtable["C_DOC_VER"];
            tfC_RAJ.Text = (string)hashtable["C_RAJ"];
            tfC_REG.Text = (string)hashtable["C_REG"];
            tfC_STI_ORIG.Text = (string)hashtable["C_STI_ORIG"];

        }

        private void btnOpenNewXLSFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();

            openFileDlg.FileName = "Document"; // Default file name
            openFileDlg.DefaultExt = ".txt"; // Default file extension
            openFileDlg.Filter = "MS XLS documents (.xls)|*.xls"; // Filter files by extension

            Nullable<bool> result = openFileDlg.ShowDialog();

            if (result == true)
            {
                // Open document
                xlsFileName = openFileDlg.FileName;
                //txtFileName;
                txtFileName.Text = xlsFileName;
                //btnDoIt.
                this.btnDoIt.IsEnabled = true;
            }
        }


        private bool fillConvertorPreparedStructure()
        {
            bool success = false;
            int iPERIOD_YEAR = 0;

            _preparedSettings4Convertation.TIN = tfTIN.Text;
            _preparedSettings4Convertation.C_DOC = tfC_DOC.Text;
            _preparedSettings4Convertation.C_DOC_CNT = tfC_DOC_CNT.Text;
            _preparedSettings4Convertation.C_DOC_SUB = tfC_DOC_SUB.Text;
            _preparedSettings4Convertation.PERIOD_MONTH = tfPERIOD_MONTH.Text;
            _preparedSettings4Convertation.D_FILL = tfD_FILL.Text;
            _preparedSettings4Convertation.HSTI = tfHSTI.Text;
            _preparedSettings4Convertation.HTIN = tfHTIN.Text;
            _preparedSettings4Convertation.HNAME = tfHNAME.Text;
            _preparedSettings4Convertation.HBOSS = tfHBOS.Text;
            _preparedSettings4Convertation.HFILL = tfHFILL.Text;
            _preparedSettings4Convertation.C_DOC_STAN = tfC_DOC_CNT.Text;
            _preparedSettings4Convertation.C_DOC_VER = tfC_DOC_VER.Text;
            _preparedSettings4Convertation.C_RAJ = tfC_RAJ.Text;
            _preparedSettings4Convertation.C_REG = tfC_REG.Text;
            _preparedSettings4Convertation.C_STI_ORIG = tfC_STI_ORIG.Text;


             success = Int32.TryParse(tfPERIOD_YEAR.Text, out iPERIOD_YEAR);//tfPERIOD_YEAR.Text
            if (success)
            {
                _preparedSettings4Convertation.PERIOD_YEAR = iPERIOD_YEAR;
            }
            else
            {
                _preparedSettings4Convertation.PERIOD_YEAR = -1;
            }

            var typeOfDocument = cmbDocumentType.SelectedItem;
            var typeOfPeriod = cbPERIOD_TYPE.SelectedItem;


            if (typeOfDocument != null)
            {
                
                Type typeOf = typeOfDocument.GetType();
                Console.WriteLine(typeOf.Name.ToString());

                //dictionaryDocumentType
                var keyOfTypeOfDocument = dictionaryDocumentType.FirstOrDefault(x => x.Value.Equals( typeOfDocument)).Key;

                if(keyOfTypeOfDocument != EnumNumberOfSchemaReport.noschema)
                {
                    success &= true;
                }
                else
                {
                    success = false;
                }
                _preparedSettings4Convertation.DOCNUMBER = keyOfTypeOfDocument;
            }
            else
            {
                success = false;
            }
            if (typeOfPeriod != null)
            {
                Type typeOf = typeOfPeriod.GetType();
                Console.WriteLine(typeOf.Name.ToString());

                //dictionaryPeriodOfKods
                var keyOfTypeOfPeriod = dictionaryPeriodOfKods.FirstOrDefault(x => x.Value.Equals( typeOfPeriod)).Key;

                _preparedSettings4Convertation.PERIOD_TYPE = keyOfTypeOfPeriod;

                success &= true;
            }
            else
            {
                success = false;
            }


            success &= true;

            return success;
        }

        private void btnDoIt_Click(object sender, RoutedEventArgs e)
        {
            bool permitAction = false;
            permitAction = fillConvertorPreparedStructure();
        }
    }
}
