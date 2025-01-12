using System;
using System.Collections.Generic;
using System.IO;
using AI.Model;
using GameCore.Cards;

namespace AI.Provincial
{
    static class Data
    {
        static float[] priorityList;
        static object obj = new object();

        //static char sep = Path.DirectorySeparatorChar;
        //static string path = $"..{sep}..{sep}..{sep}AI{sep}Provincial{sep}data{sep}priority.txt";
        //static string path = $"..{sep}..{sep}..{sep}AI{sep}priority.txt";
        static string path = BuyAgenda.DirectoryPath + BuyAgenda.sep + ".." + BuyAgenda.sep + "priority.txt";

        // list is indexed by CardType
        // priority list is computed only once
        public static float[] GetPriorityList()
        {
            // avoiding locking when its unnecesarry
            if (priorityList != null)
                return priorityList;

            lock (obj)
            {
                if (priorityList == null)
                    priorityList = getPriorityList();
                return priorityList;
            }
        }

        // list is indexed by CardType
        private static float[] getPriorityList()
        {
            var list = new List<string>();
            using (var reader = new StreamReader(path))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    list.Add(line);
                }
            } 

            var array = new float[Enum.GetNames(typeof(CardType)).Length];

            for (int i = 0; i < list.Count; i++)
                if (Enum.TryParse(list[i], out CardType type))
                    array[(int)type] = (list.Count - i) * 2;
            return array;
        }
    }
}
