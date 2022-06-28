using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Nagaduvach
{
    [Serializable]
    public class UserState
    {
        public long UserID { get; set; }
        public int State { get; set; }
        public UserState(long userid,int state)
        {
            UserID = userid;
            State = state;
        }
        public UserState(long userid)
        {
            UserID = userid;
            State = 0;
        }
        public UserState() { }

    }
    [Serializable]
    public class UserStates
    {
        public List<UserState> Userstates { get; set; } = new List<UserState>();

        public UserStates() { }
        public long GetUserState(long userid)
        {
            int state = 0;

            foreach(UserState userstate in Userstates)
            {
                if (userstate.UserID == userid)
                {
                    state = userstate.State;
                    return state;
                }
            }

            Userstates.Add(new UserState(userid));
            return state;
        }
        public void SetUserState(long userid, int state)
        {
            foreach (UserState userstate in Userstates)
            {
                if (userstate.UserID == userid)
                {
                    userstate.State= state;
                }
            }
        }

        public void SerializeStates()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(UserStates));
            File.Delete(@"..\files\userstates.xml");
            using (FileStream fs = new FileStream(@"..\files\userstates.xml", FileMode.OpenOrCreate))
            {

                xmlSerializer.Serialize(fs, this);
            }
        }
        public UserStates DeSerializeStates()
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(UserStates));
                using (FileStream fs = new FileStream(@"..\files\userstates.xml", FileMode.OpenOrCreate))
                {
                    return (UserStates)xmlSerializer.Deserialize(fs);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return null;
        }
    }
}
