﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;

namespace TransXLS2XML
{
    public class INIFiles
    {
        private string _path; //Имя файла.


        [DllImport("kernel32", CharSet = CharSet.Auto)] // Подключаем kernel32.dll и описываем его функцию WritePrivateProfilesString
        static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

        [DllImport("kernel32", CharSet = CharSet.Auto)] // Еще раз подключаем kernel32.dll, а теперь описываем функцию GetPrivateProfileString
        static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern uint GetPrivateProfileSection(string lpAppName, IntPtr lpReturnedString, uint nSize, string lpFileName);


        public INIFiles()
        {

        }


        public INIFiles(string IniPath) : this()
        {
            try
            {
                _path = new FileInfo(IniPath).FullName.ToString();
            } catch ( IOException ex)
            {

            }
        }

        //Читаем ini-файл и возвращаем значение указного ключа из заданной секции.
        public string ReadINI(string Section, string Key)
        {
            var RetVal = new StringBuilder(255);
            GetPrivateProfileString(Section, Key, "", RetVal, 255, _path);
            return RetVal.ToString();
        }

        //Записываем в ini-файл. Запись происходит в выбранную секцию в выбранный ключ.
        public void Write(string Section, string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, _path);
        }

        //Удаляем ключ из выбранной секции.
        public void DeleteKey(string Key, string Section = null)
        {
            Write(Section, Key, null);
        }

        //Удаляем выбранную секцию
        public void DeleteSection(string Section = null)
        {
            Write(Section, null, null);
        }

        //Проверяем, есть ли такой ключ, в этой секции
        public bool KeyExists(string Section, string Key)
        {
            return ReadINI(Section, Key).Length > 0;
        }

        //читаем в массив все пары ключей-значнией в данной секции
        public bool GetPrivateProfileSection(string appName, string fileName, out string[] section)
        {
            section = null;

            if (!System.IO.File.Exists(fileName))
                return false;

            const uint MAX_BUFFER = 32767;

            IntPtr pReturnedString = Marshal.AllocCoTaskMem((int)MAX_BUFFER * sizeof(char));

            uint bytesReturned = GetPrivateProfileSection(appName, pReturnedString, MAX_BUFFER, fileName);

            if ((bytesReturned == MAX_BUFFER - 2) || (bytesReturned == 0))
            {
                Marshal.FreeCoTaskMem(pReturnedString);
                return false;
            }
            string returnedString = Marshal.PtrToStringAuto(pReturnedString, (int)(bytesReturned - 1));

            section = returnedString.Split('\0');

            Marshal.FreeCoTaskMem(pReturnedString);
            return true;
        }



    }
}
