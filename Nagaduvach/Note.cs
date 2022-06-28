using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Nagaduvach
{
    [Serializable]
    public class Note
    {
        public long UserID { get; set; }
        public string Text { get; set; }
        public Note() { }
        public Note(long UserID, string Text)
        {
            this.UserID = UserID;
            this.Text = Text;
        }
    }
    [Serializable]
    public class Notes
    {
        public List<Note> Notelist { get; set; } = new List<Note>();
        public Notes() { }

        public void SerializeNotes()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Notes));
            File.Delete(@"..\files\notes.xml");
            using (FileStream fs = new FileStream(@"..\files\notes.xml", FileMode.OpenOrCreate))
            {
                xmlSerializer.Serialize(fs, this);
            }
        }
        public Notes DeSerializeNotes()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Notes));
            using (FileStream fs = new FileStream(@"..\files\notes.xml", FileMode.OpenOrCreate))
            {
                return (Notes)xmlSerializer.Deserialize(fs);
            }
        }

    }
}
