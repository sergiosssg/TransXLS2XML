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
        public string HBOSS;
        public string HFILL;
    }


    public class ConvertorPreparedInfo
    {
        private Dictionary<string, string> _parametersValues;
        private Dictionary<string, string> _parametersNewValues; // after changing value by user in program proper key gain this new value, so ...
        private Dictionary<string, bool> _parametersChangedKeyIsName;
        //private Dictionary<bool, ISet<string>> _parametersChangedKeyIsChanged;
        private Dictionary<ConvertorPreparedLocationParams, ISet<string>> _parametersTypesBodyOrHead;
        private Dictionary<ConvertorPreparedLocationParamsOfBodySection, ISet<string>> _parametersTypesBodySection;
        private string _sPathToIni;
        private INIFiles _iniFile;
        private  static string[] _propertiesOrder; // order which keys should be keep  to follow each after other
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
            _parametersChangedKeyIsName = new Dictionary<string, bool>();  // points to parameters, which value was changed already
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
                arrayOfKeys = _propertiesOrder;
            }
            else
            {
                _propertiesOrder = arrayOfKeys;
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
            return _propertiesOrder;
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

            if(sFileName != null && sFileName.Length != 0)
            {
                this._sPathToIni = sFileName;
                _iniFile = new INIFiles(this._sPathToIni);
            }

            var hashtable = new Hashtable();
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
            string sSectionOfDOCNUMBER = _propertiesOrder[0].Substring(0, _propertiesOrder[0].IndexOf('.'));

            if (_iniFile.KeyExists(sSectionOfDOCNUMBER, _propertiesOrder[0])) // _propertiesOrder[0] == "TYPE.DOCNUMBER"
            {
                _sDOCNUMBER = _iniFile.ReadINI(sSectionOfDOCNUMBER, _propertiesOrder[0]);
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

            //preparedSettings4Convertation.DOCNUMBER;
            string strDOCNUMBER;
            switch (preparedSettings4Convertation.DOCNUMBER){
                case EnumNumberOfSchemaReport.J0147105:
                    strDOCNUMBER = "J0147105";
                    break;
                case EnumNumberOfSchemaReport.J1312002:
                    strDOCNUMBER = "J1312002";
                    break;
            }

            //preparedSettings4Convertation.C_DOC;
            //preparedSettings4Convertation.C_DOC_CNT;
            //preparedSettings4Convertation.C_DOC_STAN;
            //preparedSettings4Convertation.C_DOC_SUB;
            //preparedSettings4Convertation.C_DOC_TYPE;
            //preparedSettings4Convertation.C_DOC_VER;
            //preparedSettings4Convertation.C_RAJ;
            //preparedSettings4Convertation.C_REG;
            //preparedSettings4Convertation.C_STI_ORIG;
            //preparedSettings4Convertation.D_FILL;
            //preparedSettings4Convertation.HBOSS;
            //preparedSettings4Convertation.HFILL;
            //preparedSettings4Convertation.HNAME;
            //preparedSettings4Convertation.HSTI;
            //preparedSettings4Convertation.HTIN;
            //preparedSettings4Convertation.LINKED_DOCS;
            //preparedSettings4Convertation.PERIOD_MONTH;

            string strPERIOD_TYPE;
            switch (preparedSettings4Convertation.PERIOD_TYPE)
            {
                case EnumPeriodOfKod.year:
                    strPERIOD_TYPE = "";
                    break;
                case EnumPeriodOfKod.semiyear:
                    strPERIOD_TYPE = "";
                    break;
                case EnumPeriodOfKod.quartal:
                    strPERIOD_TYPE = "";
                    break;
                case EnumPeriodOfKod.nine_months:
                    strPERIOD_TYPE = "";
                    break;
                case EnumPeriodOfKod.month:
                    strPERIOD_TYPE = "";
                    break;
            }

            string strPERIOD_YEAR = preparedSettings4Convertation.PERIOD_YEAR.ToString();


            //preparedSettings4Convertation.SOFTWARE;
            //preparedSettings4Convertation.TIN;


            //preparedSettings4Convertation.


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
                    //_propertiesOrder = sArrayOfKeys;
                    return sArrayOfKeys;
                }
            }
            //_propertiesOrder = sArrayOfKeys;
            return sArrayOfKeys;
        }


        private void InitializerByDefaultKeysForSectionBodyOrHeadToSeparateSet()
        {
            foreach (var sOneProperty in _propertiesOrder)
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
