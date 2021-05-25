using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;




namespace TransXLS2XML
{


    public enum ConvertorPreparedLocationParams : uint
    {
        BODY = 0x20, HEAD = 0x30
    }

    public enum ConvertorPreparedLocationParamsOfBodySection : uint
    {
        BEFORE = 0x80, AFTER = 0xC0
    }

    public struct PreparedSettings4ConvertationStruct
    {
        public EnumNumberOfSchemaReport DOCNUMBER;
        public string TIN;
        public string C_DOC;
        public string C_DOC_SUB;
        public string C_DOC_VER;
        public string C_DOC_TYPE;
        public string C_DOC_CNT;
        public string C_REG;
        public string C_RAJ;
        public EnumPeriodOfKod PERIOD_TYPE;
        public int PERIOD_YEAR;
        public string C_STI_ORIG;
        public string C_DOC_STAN;
        public string LINKED_DOCS;
        public string D_FILL;
        public string SOFTWARE;
        public string PERIOD_MONTH;
        public string HSTI;
        public string HTIN;
        public string HNAME;
        public string HBOS;
        public string HFILL;
    }


    public class ConvertorPreparedInfo
    {
        private const string sDOCNUMBER_DEFAULT_NUMBER = "J0147105";

        private Dictionary<string, string> _parametersValues;
        private Dictionary<string, string> _parametersNewValues; // after changing value by user in program proper key gain this new value, so ...
        private Dictionary<string, bool> _parametersChangedKeyIsName;
        //private Dictionary<bool, ISet<string>> _parametersChangedKeyIsChanged;
        private Dictionary<ConvertorPreparedLocationParams, ISet<string>> _parametersTypesBodyOrHead;
        private Dictionary<ConvertorPreparedLocationParamsOfBodySection, ISet<string>> _parametersTypesBodySection;
        private string _sPathToIni;
        private INIFiles _iniFile;
        //private  static string[] _propertiesOrder; // order which keys should be keep  to follow each after other
        private string _sDOCNUMBER;
        private string _sNewDOCNUMBER;
        private bool _DOCNUMBERchanged;


        public ConvertorPreparedInfo()
        {
            _DOCNUMBERchanged = false;
            _sNewDOCNUMBER = "";
            //HashSet<string> setOfProperties = new HashSet<string>();
            _parametersValues = new Dictionary<string, string>();
            _parametersNewValues = new Dictionary<string, string>();
            _parametersChangedKeyIsName = new Dictionary<string, bool>();
            //_parametersChangedKeyIsChanged = new Dictionary<bool, ISet<string>>();
            _parametersTypesBodyOrHead = new Dictionary<ConvertorPreparedLocationParams, ISet<string>>();
            _parametersTypesBodyOrHead.Add( ConvertorPreparedLocationParams.HEAD , new HashSet<string>());
            _parametersTypesBodyOrHead.Add( ConvertorPreparedLocationParams.BODY , new HashSet<string>());
            _parametersTypesBodySection = new Dictionary<ConvertorPreparedLocationParamsOfBodySection, ISet<string>>();
            _parametersTypesBodySection.Add(ConvertorPreparedLocationParamsOfBodySection.BEFORE , new HashSet<string>());
            _parametersTypesBodySection.Add(ConvertorPreparedLocationParamsOfBodySection.AFTER, new HashSet<string>());
        }

        public ConvertorPreparedInfo(string sPathToINIfile) : this()
        {
            this._sPathToIni = sPathToINIfile;
            _iniFile = new INIFiles(this._sPathToIni);
        }

        public ConvertorPreparedInfo(string[] sArrayOfKeys) : this()
        {
            InitializerByDefaultKeysForSectionBodyOrHead(sArrayOfKeys);
            InitializerByDefaultKeysForSectionBodyOrHeadToSeparateSet();
        }

        public ConvertorPreparedInfo(string sPathToINIfile, string[] sArrayOfKeys) : this()
        {
            this._sPathToIni = sPathToINIfile;
            _iniFile = new INIFiles(this._sPathToIni);
            InitializerByDefaultKeysForSectionBodyOrHead(sArrayOfKeys);
            InitializerByDefaultKeysForSectionBodyOrHeadToSeparateSet();
        }

        public void setTypesOfElements(string[] arrayOfKeys = null, 
            string sBODYbefore = "HSTI, HTIN, HNAME", string sBODYafter = "HBOS, HFILL")
        {
            if (InitializerByDefaultKeysForSectionBodyOrHead(arrayOfKeys) == null)
            {
                arrayOfKeys = InitializerByDefaultKeysForSectionBodyOrHead();
            }
            else
            {
                arrayOfKeys = InitializerByDefaultKeysForSectionBodyOrHead();
            }

            InitializerByDefaultKeysForSectionBodyOrHeadToSeparateSet();

            ISet<string> setOfHeadKeys = _parametersTypesBodyOrHead[ConvertorPreparedLocationParams.HEAD];
            var arrayOfKeysHEAD = arrayOfKeys.Where ( el => (el.Contains('.') && el.Substring(0, el.IndexOf('.')).Equals("HEAD"))).Select( el => el.Substring(el.IndexOf('.') + 1));
            foreach(var elHEAD in arrayOfKeysHEAD) { setOfHeadKeys.Add(elHEAD); }
            ISet<string> setOfBodyKeys = _parametersTypesBodyOrHead[ConvertorPreparedLocationParams.BODY];
            var arrayOfKeysBODY = arrayOfKeys.Where ( el => (el.Contains('.') && el.Substring(0, el.IndexOf('.')).Equals("BODY"))).Select( el => el.Substring(el.IndexOf('.') + 1));
            foreach (var elBODY in arrayOfKeysBODY) { setOfBodyKeys.Add(elBODY); }
            string[] sArrayBODYbefore = sBODYbefore.Split(','); //for(int ii = 0; ii < sArrayBODYbefore.Length; ii++) { sArrayBODYbefore[ii] = sArrayBODYbefore[ii].Trim(); }
            string[] sArrayBODYafter = sBODYafter.Split(','); //for (int ii = 0; ii < sArrayBODYafter.Length; ii++) { sArrayBODYafter[ii] = sArrayBODYafter[ii].Trim(); }

            ISet<string> setOfBodyBefore = _parametersTypesBodySection[ConvertorPreparedLocationParamsOfBodySection.BEFORE];
            foreach(var elBODYbefore in sArrayBODYbefore) { setOfBodyBefore.Add(elBODYbefore.Trim()); }

            ISet<string> setOfBodyAfter = _parametersTypesBodySection[ConvertorPreparedLocationParamsOfBodySection.AFTER];
            foreach (var elBODYafter in sArrayBODYafter) { setOfBodyAfter.Add(elBODYafter.Trim()); }
        }

        public bool IsThisKeyChanged(string sAttr)
        {
            return _parametersChangedKeyIsName[sAttr];
        }

        // set attribute sAttr as changed according bTogleChanged
        public void SetThisKeyAsChanged(string sAttr, bool bTogleChanged = false)
        {
            _parametersChangedKeyIsName[sAttr] = bTogleChanged;
        }

        public void SetThisKeyNewValue(string sAttr, string sValue)
        {
            if (sAttr.EndsWith("DOCNUMBER"))
            {
                if (!_sDOCNUMBER.Equals(sValue))
                {
                    _sNewDOCNUMBER = sValue.Trim();
                    _DOCNUMBERchanged = true;
                }
            }
            else
            {
                string sNewValue = sValue.Trim();
                string sOldValue = _parametersValues[sAttr];
                if (!sOldValue.Equals(sNewValue))
                {
                    if (_parametersNewValues.ContainsKey(sAttr))
                    {
                        _parametersNewValues[sAttr] = sValue.Trim();
                    }
                    else
                    {
                        _parametersNewValues.Add(sAttr, sValue.Trim());
                    }
                    SetThisKeyAsChanged(sAttr, true);
                }
            }
        }


        public string DOCNUMBER
        {
            set
            {
                string sNewDOCNUMBER = value.Trim().ToUpper();
                if (!_sDOCNUMBER.Equals(sNewDOCNUMBER))
                {
                    _sNewDOCNUMBER = sNewDOCNUMBER;
                    _DOCNUMBERchanged = true;
                    _sDOCNUMBER = _sNewDOCNUMBER;
                }
            }
            get
            {
                return _sDOCNUMBER;
            }
        }


        public string PathToINIfile
        {
            set
            {
                this._sPathToIni = value;
                _iniFile = new INIFiles(this._sPathToIni);
            }
        }

        // returns keys with its prefix string with dot split prefix and key
        public string[] getFullKeysNames()
        {
            return InitializerByDefaultKeysForSectionBodyOrHead();
        }


        public Dictionary<string, string> WeAreAtHead
        {
            get => _parametersValues;
        }

        public Dictionary<string, string> WeAreAtBody
        {
            get => _parametersValues;
        }

        public Dictionary<string, string> WeAreBefore
        {
            get => _parametersValues;
        }

        public Dictionary<string, string> WeAreAfter
        {
            get => _parametersValues;
        }

        public bool loadProperiesFromINIfile(string sFileName = null)
        {
            bool resultOfFunction = false;

            if (sFileName != null && sFileName.Length != 0)
            {
                this._sPathToIni = sFileName;
                _iniFile = new INIFiles(this._sPathToIni);
            }

            var hashtable = new Hashtable();
            //instead of using inner class variable  I use InitializerByDefaultKeysForSectionBodyOrHead()
            var arrayKeyStrings = InitializerByDefaultKeysForSectionBodyOrHead();
            int i = 0;

            foreach (string s in arrayKeyStrings)
            {
                string sKey = s.Substring(s.IndexOf('.') + 1);
                string sSection = s.Substring(0, s.IndexOf('.'));
                string sValue;

                if (_iniFile.KeyExists(sSection, s))
                {
                    resultOfFunction = (i > 0) ? resultOfFunction & true : true;
                    sValue = _iniFile.ReadINI(sSection, s);
                    hashtable.Add(sKey, sValue);
                    i++;
                }
                else { resultOfFunction = false; }
            }
            string sSectionOfDOCNUMBER = arrayKeyStrings[0].Substring(0, arrayKeyStrings[0].IndexOf('.'));

            if (_iniFile.KeyExists(sSectionOfDOCNUMBER, arrayKeyStrings[0])) // _propertiesOrder[0] == "TYPE.DOCNUMBER"
            {
                _sDOCNUMBER = _iniFile.ReadINI(sSectionOfDOCNUMBER, arrayKeyStrings[0]);
                resultOfFunction = resultOfFunction & true;
            }
            else { resultOfFunction = false; }

            return resultOfFunction;
        }

        public bool saveProperiesToINIfile(string sFileName = null)
        {
            bool resultOfFunction = false;

            if (sFileName != null && sFileName.Length != 0)
            {
                this._sPathToIni = sFileName;
                _iniFile = new INIFiles(this._sPathToIni);
            }
            // determine Whether values, to store in ini files changed, and hence we have to store them
            bool boolNeedToSave = _DOCNUMBERchanged;
            if (!boolNeedToSave)
            {
                foreach(var pairChangedKeyIsName in _parametersChangedKeyIsName)
                {
                    if (pairChangedKeyIsName.Value)
                    {
                        boolNeedToSave = true;
                        break;
                    }
                }
            }

            if (boolNeedToSave) // perform storing of changed parameters (main job) ...
            {
                if (_DOCNUMBERchanged)
                {
                    _iniFile.Write("TYPE", "TYPE.DOCNUMBER", _sNewDOCNUMBER);
                    _sDOCNUMBER = _sNewDOCNUMBER;
                    _sNewDOCNUMBER = null;
                    _DOCNUMBERchanged = false;
                    resultOfFunction = true;
                }
                foreach (var pairChangedKeyIsName in _parametersChangedKeyIsName)
                {
                    string sKey;
                    if (pairChangedKeyIsName.Value)
                    {
                        sKey = pairChangedKeyIsName.Key;
                        string sKeyPrefix = "";

                        ConvertorPreparedLocationParams whereToConvertorLocation = (ConvertorPreparedLocationParams) GetKeyFromValueConvertorPreparedLocationParams(sKey, in _parametersTypesBodyOrHead);

                        if(whereToConvertorLocation == ConvertorPreparedLocationParams.BODY)
                        {
                            sKeyPrefix = "BODY";
                        }
                        else if (whereToConvertorLocation == ConvertorPreparedLocationParams.HEAD)
                        {
                            sKeyPrefix = "HEAD";
                        }

                        _iniFile.Write( sKeyPrefix, sKeyPrefix + "." + sKey, _parametersNewValues[sKey]);
                        _parametersValues[sKey] = _parametersNewValues[sKey];

                        _parametersNewValues[sKey] = "";

                        SetThisKeyAsChanged(sKey, false);
                        resultOfFunction = true;

                    }
                }
            }
            // this block for clearing togles pointing that we have changed paramaters to save in ini file
            _parametersNewValues.Clear();

            return resultOfFunction;
        }


        public void loadPropertiesFromStructure(PreparedSettings4ConvertationStruct preparedSettings4Convertation)
        {
            //string strDOCNUMBER;
            switch (preparedSettings4Convertation.DOCNUMBER){
                case EnumNumberOfSchemaReport.J0147105:
                    DOCNUMBER = EnumNumberOfSchemaReport.J0147105.ToString();
                    //strDOCNUMBER = "J0147105";
                    break;
                case EnumNumberOfSchemaReport.J1312002:
                    DOCNUMBER = EnumNumberOfSchemaReport.J1312002.ToString();
                    //strDOCNUMBER = "J1312002";
                    break;
            }
            //_DOCNUMBERchanged = true;

            //_sNewDOCNUMBER = strDOCNUMBER;

            //string strPERIOD_TYPE;
            switch (preparedSettings4Convertation.PERIOD_TYPE)
            {
                case EnumPeriodOfKod.year:
                    //strPERIOD_TYPE = "год";
                    SetThisKeyNewValue("PERIOD_TYPE", "год");
                    break;
                case EnumPeriodOfKod.semiyear:
                    //strPERIOD_TYPE = "пол года";
                    SetThisKeyNewValue("PERIOD_TYPE", "пол года");
                    break;
                case EnumPeriodOfKod.quartal:
                    //strPERIOD_TYPE = "квартал";
                    SetThisKeyNewValue("PERIOD_TYPE", "квартал");
                    break;
                case EnumPeriodOfKod.nine_months:
                    //strPERIOD_TYPE = "9 месяцев";
                    SetThisKeyNewValue("PERIOD_TYPE", "9 месяцев");
                    break;
                case EnumPeriodOfKod.month:
                    //strPERIOD_TYPE = "месяц";
                    SetThisKeyNewValue("PERIOD_TYPE", "месяц");
                    break;
            }
            SetThisKeyAsChanged("PERIOD_TYPE", true);



            SetThisKeyAsChanged("PERIOD_YEAR", true);
            SetThisKeyNewValue("PERIOD_YEAR", preparedSettings4Convertation.PERIOD_YEAR.ToString());


            //"TIN";
            SetThisKeyAsChanged("TIN", true);
            SetThisKeyNewValue("TIN", preparedSettings4Convertation.TIN);

            //"C_DOC";
            SetThisKeyAsChanged("C_DOC", true);
            SetThisKeyNewValue("C_DOC", preparedSettings4Convertation.C_DOC);

            //"C_DOC_SUB";
            SetThisKeyAsChanged("C_DOC_SUB", true);
            SetThisKeyNewValue("C_DOC_SUB", preparedSettings4Convertation.C_DOC_SUB);

            //"C_DOC_VER";
            SetThisKeyAsChanged("C_DOC_VER", true);
            SetThisKeyNewValue("C_DOC_VER", preparedSettings4Convertation.C_DOC_VER);

            //"C_DOC_TYPE";
            SetThisKeyAsChanged("C_DOC_TYPE", true);
            SetThisKeyNewValue("C_DOC_TYPE", preparedSettings4Convertation.C_DOC_TYPE);

            //"C_DOC_CNT";
            SetThisKeyAsChanged("C_DOC_CNT", true);
            SetThisKeyNewValue("C_DOC_CNT", preparedSettings4Convertation.C_DOC_CNT);

            //"C_REG";
            SetThisKeyAsChanged("C_REG", true);
            SetThisKeyNewValue("C_REG", preparedSettings4Convertation.C_REG);

            //"C_RAJ";
            SetThisKeyAsChanged("C_RAJ", true);
            SetThisKeyNewValue("C_RAJ", preparedSettings4Convertation.C_RAJ);

            //"C_STI_ORIG";
            SetThisKeyAsChanged("C_STI_ORIG", true);
            SetThisKeyNewValue("C_STI_ORIG", preparedSettings4Convertation.C_STI_ORIG);

            //"C_DOC_STAN";
            SetThisKeyAsChanged("C_DOC_STAN", true);
            SetThisKeyNewValue("C_DOC_STAN", preparedSettings4Convertation.C_DOC_STAN);

            //"LINKED_DOCS";
            SetThisKeyAsChanged("LINKED_DOCS", true);
            SetThisKeyNewValue("LINKED_DOCS", preparedSettings4Convertation.LINKED_DOCS);

            //"D_FILL";
            SetThisKeyAsChanged("D_FILL", true);
            SetThisKeyNewValue("D_FILL", preparedSettings4Convertation.D_FILL);

            //"SOFTWARE";
            SetThisKeyAsChanged("SOFTWARE", true);
            SetThisKeyNewValue("SOFTWARE", preparedSettings4Convertation.SOFTWARE);

            //"HSTI";
            SetThisKeyAsChanged("HSTI", true);
            SetThisKeyNewValue("HSTI", preparedSettings4Convertation.HSTI);

            //"HTIN";
            SetThisKeyAsChanged("HTIN", true);
            SetThisKeyNewValue("HTIN", preparedSettings4Convertation.HTIN);

            //"HNAME";
            SetThisKeyAsChanged("HNAME", true);
            SetThisKeyNewValue("HNAME", preparedSettings4Convertation.HNAME);

            //"HBOS";
            SetThisKeyAsChanged("HBOS", true);
            SetThisKeyNewValue("HBOS", preparedSettings4Convertation.HBOS);

            //"HFILL";
            SetThisKeyAsChanged("HFILL", true);
            SetThisKeyNewValue("HFILL", preparedSettings4Convertation.HFILL);


        }


        public bool IsThisHead(string sAttr) => _parametersTypesBodyOrHead[ConvertorPreparedLocationParams.HEAD].Contains(sAttr);

        public bool IsThisBody(string sAttr) => _parametersTypesBodyOrHead[ConvertorPreparedLocationParams.BODY].Contains(sAttr);

        public bool IsThisBefore(string sAttr) => _parametersTypesBodySection[ConvertorPreparedLocationParamsOfBodySection.BEFORE].Contains(sAttr);

        public bool IsThisAfter(string sAttr) => _parametersTypesBodySection[ConvertorPreparedLocationParamsOfBodySection.AFTER].Contains(sAttr);

        public string getValue( string sKey) => _parametersValues[sKey];


        private string[] InitializerByDefaultKeysForSectionBodyOrHead(string[] sArrayOfKeys = null)
        {
            string[] sArrayOfKeysRet;

            if (sArrayOfKeys == null || sArrayOfKeys.Length == 0)
            {
                sArrayOfKeys = new string[21];
                {
                    sArrayOfKeys[0] = "TYPE.DOCNUMBER";
                    sArrayOfKeys[1] = "HEAD.TIN";
                    sArrayOfKeys[2] = "HEAD.C_DOC";
                    sArrayOfKeys[3] = "HEAD.C_DOC_SUB";
                    sArrayOfKeys[4] = "HEAD.C_DOC_VER";
                    sArrayOfKeys[5] = "HEAD.C_DOC_TYPE";
                    sArrayOfKeys[6] = "HEAD.C_DOC_CNT";
                    sArrayOfKeys[7] = "HEAD.C_REG";
                    sArrayOfKeys[8] = "HEAD.C_RAJ";
                    sArrayOfKeys[9] = "HEAD.PERIOD_TYPE";
                    sArrayOfKeys[10] = "HEAD.PERIOD_YEAR";
                    sArrayOfKeys[11] = "HEAD.C_STI_ORIG";
                    sArrayOfKeys[12] = "HEAD.C_DOC_STAN";
                    sArrayOfKeys[13] = "HEAD.LINKED_DOCS";
                    sArrayOfKeys[14] = "HEAD.D_FILL";
                    sArrayOfKeys[15] = "HEAD.SOFTWARE";
                    sArrayOfKeys[16] = "BODY.HSTI";
                    sArrayOfKeys[17] = "BODY.HTIN";
                    sArrayOfKeys[18] = "BODY.HNAME";
                    sArrayOfKeys[19] = "BODY.HBOS";
                    sArrayOfKeys[20] = "BODY.HFILL";
                    sArrayOfKeysRet = null;
                }
            }

            sArrayOfKeysRet = sArrayOfKeys;
            //_propertiesOrder = sArrayOfKeys;
            return sArrayOfKeysRet;
        }


        private void InitializerByDefaultKeysForSectionBodyOrHeadToSeparateSet()
        {
            string[] sArrayOfKeysRet = InitializerByDefaultKeysForSectionBodyOrHead();
            foreach (var sOneProperty in sArrayOfKeysRet)
            {
                var sPrefix = sOneProperty.Substring(0, sOneProperty.IndexOf('.'));
                if (!sPrefix.Equals("TYPE"))
                {
                    var sKey = sOneProperty.Substring(sOneProperty.IndexOf('.') + 1);
                    //setOfProperties.Add(sKey);
                    _parametersChangedKeyIsName.Add(sKey, false);
                    if (sPrefix.ToUpper().Equals("HEAD"))
                    {
                        ISet<string> headSet = _parametersTypesBodyOrHead[ConvertorPreparedLocationParams.HEAD];
                        headSet.Add(sKey);
                        _parametersTypesBodyOrHead[ConvertorPreparedLocationParams.HEAD] = headSet;
                    }
                    else if (sPrefix.ToUpper().Equals("BODY"))
                    {
                        ISet<string> bodySet = _parametersTypesBodyOrHead[ConvertorPreparedLocationParams.BODY];
                        bodySet.Add(sKey);
                        _parametersTypesBodyOrHead[ConvertorPreparedLocationParams.BODY] = bodySet;
                    }
                } else
                {
                    string sDOCNUMBER = sDOCNUMBER_DEFAULT_NUMBER;// _sDOCNUMBER
                }
            }
        }





        private static uint? GetKeyFromValueConvertorPreparedLocationParams(string valueVar, in Dictionary<ConvertorPreparedLocationParams, ISet<string>> dic)
        {
            ISet<string> setOfValues;
            foreach (var keyVar in dic.Keys)
            {

                setOfValues = dic[keyVar];

                if (setOfValues.Contains(valueVar))
                {
                    return (uint)keyVar;
                }

            }
            return null;
        }

    }
}
