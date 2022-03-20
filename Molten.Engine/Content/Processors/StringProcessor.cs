﻿namespace Molten.Content
{
    public class StringProcessor : ContentProcessor
    {
        public override Type[] AcceptedTypes { get; } = new Type[] { typeof(string) };

        public override Type[] RequiredServices => null;

        protected override void OnInitialize()
        {
            AddParameter("binary", false);
            AddParameter("perline", false);
        }

        public override void OnRead(ContentContext context)
        {
            bool isBinary = context.Parameters.Get<bool>("binary");

            using (Stream stream = new FileStream(context.Filename, FileMode.Open, FileAccess.Read))
            {
                if (isBinary)
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        while (reader.BaseStream.Position < reader.BaseStream.Length)
                            context.AddOutput(reader.ReadString());
                    }
                }
                else
                {
                    bool perLine = context.Parameters.Get<bool>("perline");

                    using (StreamReader reader = new StreamReader(stream))
                    {
                        if (perLine)
                        {
                            while (!reader.EndOfStream)
                                context.AddOutput(reader.ReadLine());
                        }
                        else
                        {
                            context.AddOutput(reader.ReadToEnd());
                        }
                    }
                }
            }
        }

        public override void OnWrite(ContentContext context)
        {
            if (context.Input.TryGetValue(AcceptedTypes[0], out List<object> strings))
            {
                bool isBinary = context.Parameters.Get<bool>("binary");

                using (Stream stream = new FileStream(context.Filename, FileMode.Create, FileAccess.Write))
                {
                    if (isBinary)
                    {
                        using (BinaryWriter writer = new BinaryWriter(stream))
                        {
                            foreach (string str in strings)
                                writer.Write(str);
                        }
                    }
                    else
                    {
                        using (StreamWriter writer = new StreamWriter(stream))
                        {
                            foreach (string str in strings)
                                writer.WriteLine(str);
                        }
                    }
                }
            }
        }
    }
}
