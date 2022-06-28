using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Nagaduvach
{
    [Serializable]
    public class Reminder
    {
        public long UserID { get; set; }
        public DateTime Datetime { get; set; }
        public string Text { get; set; }
        public bool IsActive { get; set; }
        public string Repeat { get; set; }
        public Reminder() {
            IsActive = true;
        }
        public Reminder(long UserID, DateTime datetime, string Text, string Repeat)
        {
            this.UserID = UserID;
            this.Datetime = datetime;
            this.Text = Text;
            this.IsActive = true;
            this.Repeat = Repeat;
        }
        public void OnOff()
        {
            IsActive = !IsActive;
        }

        public void SerializeReminder(long id)
        {
            string path = @"..\files\newreminder"+id+ ".xml";
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Reminder));
            File.Delete(path);
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                xmlSerializer.Serialize(fs, this);
            }
        }
        public Reminder DeSerializeReminder(long id)
        {
            string path = @"..\files\newreminder" + id + ".xml";
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Reminder));
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                return (Reminder)xmlSerializer.Deserialize(fs);
            }
        }

    }
    [Serializable]
    public class Reminders
    {
        public List<Reminder> Reminderlist { get; set; } = new List<Reminder>();
        public Reminders() { }

        public void SerializeReminders()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Reminders));
            File.Delete(@"..\files\reminders.xml");
            using (FileStream fs = new FileStream(@"..\files\reminders.xml", FileMode.OpenOrCreate))
            {
                xmlSerializer.Serialize(fs, this);
            }
        }
        public Reminders DeSerializeReminders()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Reminders));
            using (FileStream fs = new FileStream(@"..\files\reminders.xml", FileMode.OpenOrCreate))
            {
                return (Reminders)xmlSerializer.Deserialize(fs);
            }
        }

    }
}
