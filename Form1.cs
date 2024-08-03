using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace itr2_glock_credit
{
    public partial class Form1 : Form
    {
        // "Money"
        static byte[] money_string_bytes = new byte[] { 0x4D, 0x6F, 0x6E, 0x65, 0x79 };
        static int money_value_offset = 26; // 0x1A

        static int reimbursement_amount = 7770;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    byte[] file_bytes = File.ReadAllBytes(openFileDialog1.FileName);

                    var position = GetPositionAfterMatch(file_bytes, money_string_bytes, 0);

                    var current_cash_bytes = new Byte[8];
                    Array.Copy(file_bytes, position + money_value_offset, current_cash_bytes, 0, 8);

                    var current_cash = BitConverter.ToInt32(current_cash_bytes, 0);

                    Debug.WriteLine(current_cash);

                    var new_total = current_cash + reimbursement_amount;

                    ReplaceBytes(file_bytes, money_string_bytes, money_value_offset, new_total, 0);

                    int backupIndex = 0;

                    while (File.Exists(openFileDialog1.FileName + ".bak." + backupIndex.ToString("D3")))
                    {
                        backupIndex++;
                        if (backupIndex > 999)
                        {
                            label1.Text = "Error: Too many backup files, can't generate backup filename";
                            return;
                        }
                    }
                    try
                    {
                        File.Copy(openFileDialog1.FileName, openFileDialog1.FileName + ".bak." + backupIndex.ToString("D3"));
                    }
                    catch (Exception ex)
                    {
                        label1.Text = "Error: Backup failed";
                        return;
                    }

                    File.WriteAllBytes(openFileDialog1.FileName, file_bytes);
                    label1.Text = "Explorer #73 Reimbursed " + reimbursement_amount + " Store Credits";
                }
                catch(Exception ex)
                {
                    label1.Text = "Error: Unknown";
                    return;
                }


            }
        }

        static int ReadInt16(BinaryReader binary_reader)
        {
            var bytes = binary_reader.ReadBytes(8);
            return BitConverter.ToInt32(bytes, 0);
        }


        static bool ReplaceBytes(byte[] sourceBytes, byte[] patternArray, int valueOffset, int newValue, int startOffset = 0)
        {
            int offset = GetPositionAfterMatch(sourceBytes, patternArray, startOffset);

            if (offset < 0)
            {
                return false;
            }

            byte[] intBytes = BitConverter.GetBytes(newValue);
            for (int i = 0; i < intBytes.Length; i++)
            {
                sourceBytes[offset + valueOffset + i] = intBytes[i];
            }

            return true;
        }

        static string BytesToHexString(byte[] bytes)
        {
            string result = "{ ";
            foreach (byte b in bytes)
            {
                result += $"0x{ b:x2}, ";
            }
            result += " }";
            return result;
        }

        static int GetPositionAfterMatch(byte[] data, byte[] pattern, int startOffest = 0)
        {
            try
            {
                for (int i = startOffest; i < data.Length - pattern.Length; i++)
                {
                    bool match = true;
                    for (int k = 0; k < pattern.Length; k++)
                    {
                        if (data[i + k] != pattern[k])
                        {
                            match = false;
                            break;
                        }
                    }
                    if (match)
                    {
                        return i + pattern.Length;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
            }
            return -1;
        }
    }
}
