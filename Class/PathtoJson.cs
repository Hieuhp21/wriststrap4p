using Newtonsoft.Json;

namespace WEB_SHOW_WRIST_STRAP
{
    public class Year
    {
        public string year;
        public List<Moth> LsMoth;
    }

    public class Moth
    {
        public string moth;
        public List<Day> Lsday;
    }

    public class Day
    {
        public string day;
        public List<Room> LsRoom;
    }

    public class Room
    {
        public string name;
        public string idroom;
        public List<string> LsLEDcsv;
    }

    public class PathtoJson
    {
        public static string GetJstlscsv(string searchFolder, string[] filters, int levelfile, string listroom)
        {
            if (listroom == null) return "[]";

            bool isRecursive = true;

            List<string> LsRoomuser = new List<string>();
            LsRoomuser = listroom.Split(',').ToList();



            List<string> list = GetListFilelv(searchFolder, filters, isRecursive, levelfile);
            List<string[]> lsfile = new List<string[]>();

            List<string> yearas = new List<string>();

            foreach (string s in list)
            {
                string[] item = s.Split('\\');
                lsfile.Add(item);
                yearas.Add(item[0]);
            }

            yearas = yearas.Distinct().ToList();

            List<Year> Lsyear = new List<Year>();

            foreach (string year in yearas)
            {
                Year Year1 = new Year();
                Year1.year = year;

                List<string[]> datayaer = new List<string[]>();

                foreach (var y in lsfile) { if (y[0] == year) datayaer.Add(y); }

                List<string> moths = new List<string>();

                foreach (var s in datayaer)
                {
                    moths.Add(s[1]);
                }
                moths = moths.Distinct().ToList();
                List<Moth> LsMoth = new List<Moth>();

                foreach (var moth in moths)
                {
                    Moth Moth1 = new Moth();
                    Moth1.moth = moth;

                    List<string[]> datamoth = new List<string[]>();

                    foreach (var m in datayaer) { if (m[1] == moth) datamoth.Add(m); }

                    List<string> days = new List<string>();

                    foreach (var s in datamoth)
                    {
                        days.Add(s[2]);
                    }

                    days = days.Distinct().ToList();

                    List<Day> LsDay = new List<Day>();

                    foreach (var s in days)
                    {
                        Day Day1 = new Day();
                        Day1.day = s;

                        List<string[]> dataday = new List<string[]>();

                        foreach (var d in datamoth) { if (d[2] == s) dataday.Add(d); }

                        List<Room> LsRoom = new List<Room>();

                        foreach (var r in LsRoomuser)
                        {
                            Room Room1 = new Room();
                            Room1.idroom = r;

                            List<string[]> dataroom = new List<string[]>();

                            foreach (var d2 in dataday) { if (d2[3].Contains("ROOM_" + r +"_")) dataroom.Add(d2); }

                            Room1.LsLEDcsv = new List<string>();

                            foreach (var s3 in dataroom)
                            {
                                string path = "";
                                for (int i = 0; i < s3.Length; i++)
                                {
                                    path += i == 0 ? s3[i] : @"/" + s3[i];
                                }
                                Room1.LsLEDcsv.Add(path);
                            }

                            if (dataroom.Count > 0)
                            {
                                Room1.name = dataroom[0][3];

                                LsRoom.Add(Room1);
                            }
                        }

                        Day1.LsRoom = LsRoom;
                        LsDay.Add(Day1);
                    }

                    Moth1.Lsday = LsDay;
                    LsMoth.Add(Moth1);
                }

                Year1.LsMoth = LsMoth;
                Lsyear.Add(Year1);
            }
            return JsonConvert.SerializeObject(Lsyear);
        }

        public static List<string> GetListFilelv(string searchFolder, String[] filters, bool isRecursive, int levelfile)
        {
            string[] Lsfilecsv = GetFilesFrom(searchFolder, filters, isRecursive);
            List<string> list = new List<string>();
            foreach (string path in Lsfilecsv)
            {
                if (path.Split('\\').Length == levelfile)
                {
                    list.Add(path);
                }
            }

            return list;
        }

        public static string[] GetFilesFrom(string searchFolder, String[] filters, bool isRecursive)
        {
            List<String> filesFound = new List<String>();
            var searchOption = isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            foreach (var filter in filters)
            {
                filesFound.AddRange(Directory.GetFiles(searchFolder, String.Format("*.{0}", filter), searchOption));
            }
            for (int i = 0; i < filesFound.Count; i++)
            {
                filesFound[i] = filesFound[i].Replace(searchFolder, string.Empty);
            }
            return filesFound.ToArray();
        }
    }
}
