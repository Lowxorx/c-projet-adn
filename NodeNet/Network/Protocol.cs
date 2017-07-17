using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeNet.Network
{
    // Format Protocole
    // [SOH] ([x][x][x][x]) ([y][y][y]) ... [data] ... [EOT]
    // Début   Taille         Code          données    Fin
    public class Protocol
    {
        public enum code { getStatus = 100, sendStatus = 101, sendData = 200}

        public byte[] encapsulate(byte[] data, code action)
        {
            byte[] formatdata = new byte[data.Length + 9];

            formatdata[0] = 1;

            byte[] size = BitConverter.GetBytes(formatdata.Length);
            System.Array.Copy(size, 0, formatdata, 1, size.Length);

            byte[] code = BitConverter.GetBytes(Convert.ToInt16(action));
            System.Array.Copy(code, 0, formatdata, 5, 3);

            System.Array.Copy(data, 0, formatdata, 7, data.Length);

            formatdata[formatdata.Length - 1] = 4;

            printbyte(formatdata);

            return formatdata;
        }

        public String decapsulate(byte[] frame)
        {
            Console.WriteLine(((char)frame[0]).ToString());

            if ((char)frame[0] == 1)
            {
                Console.WriteLine("First Byte OK");

                byte[] datasize = new byte[4];
                Array.Copy(frame, 1, datasize, 0, 4);
                int size = Convert.ToInt32(datasize);

                if (frame[size] == 4)
                {
                    Console.WriteLine("End Byte OK");
                    byte[] datacode = new byte[3];
                    Array.Copy(frame, 4, datacode, 0, 3);
                    code action = (code)Convert.ToInt16(datacode);


                    byte[] data = new byte[size - 9];
                    Array.Copy(frame, 7, data, 0, size - 8);

                    switch (action)
                    {
                        case Protocol.code.sendData:
                            break;
                        case Protocol.code.getStatus:
                            break;
                        case Protocol.code.sendStatus:
                            break;
                    }

                    return action.ToString() +" "+ System.Text.Encoding.Default.GetString(data);

                }
                else
                    throw new Exception("End Byte NOK");     
            }
            else
                throw new Exception("First Byte NOK");
        }

        private static void printbyte(byte[] b)
        {
            for (int i = 0; i < b.Length; i++)
                Console.Write(b[i].ToString() + "|");
        }
    }
}
