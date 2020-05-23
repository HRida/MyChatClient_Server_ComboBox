using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;//
using System.Net.Sockets;//
using System.IO;//

namespace MyChatClient
{
    public partial class Form1 : Form
    {
        private NetworkStream output;
        private BinaryWriter writer;
        private BinaryReader reader;

        private string message = "";

        private Thread readThread;
        public Form1()
        {
            InitializeComponent();
            readThread = new Thread(new ThreadStart(RunClient));
            readThread.Start();
        }

        private void inputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            Action updateLabel;
            try
            {
                if (e.KeyCode == Keys.Enter)
                {
                    writer.Write("CLIENT>>> " + inputTextBox.Text);
                    // Cross-thread operation not valid: Control 'textBox1' accessed from a thread other than the thread it was created on
                    updateLabel = () => displayTextBox.Text += "\r\nCLIENT>>> " + inputTextBox.Text;
                    displayTextBox.Invoke(updateLabel);

                    inputTextBox.Clear();
                }
            }
            catch (SocketException)
            {
                updateLabel = () => displayTextBox.Text += "\nError writing object";
                displayTextBox.Invoke(updateLabel);
            }
        }
        // connect to server and display server-generated text
        public void RunClient()
        {
            TcpClient client;

            // instantiate TcpClient for sending data to server
            try
            {
                Action updateLabel = () => displayTextBox.Text += "Attempting connection\r\n";
                displayTextBox.Invoke(updateLabel);

                // Step 1: create TcpClient and connect to server
                client = new TcpClient();
                client.Connect("localhost", 5001);

                // Step 2: get NetworkStream associated with TcpClient
                output = client.GetStream();

                // create objects for writing and reading across stream
                writer = new BinaryWriter(output);
                reader = new BinaryReader(output);
                updateLabel = () => displayTextBox.Text += "\r\nGot I/O streams\r\n";
                displayTextBox.Invoke(updateLabel);
                inputTextBox.ReadOnly = false;

                // loop until server signals termination
                do
                {

                    // Step 3: processing phase
                    try
                    {
                        // read message from server
                        message = reader.ReadString();
                        String[] words = message.Split(' ');

                        foreach (string word in words)
                        {
                            updateLabel = () => comboBox1.Items.Add(word);
                            comboBox1.Invoke(updateLabel);
                        }
                         updateLabel = () => displayTextBox.Text += "\r\n" + message;
                        displayTextBox.Invoke(updateLabel);
                    }

                       // handle exception if error in reading server data
                    catch (Exception)
                    {
                        System.Environment.Exit(
                           System.Environment.ExitCode);
                    }
                } while (message != "SERVER>>> TERMINATE");
                updateLabel = () => displayTextBox.Text += "\r\nClosing connection.\r\n";
                displayTextBox.Invoke(updateLabel);
                // Step 4: close connection
                writer.Close();
                reader.Close();
                output.Close();
                client.Close();
                Application.Exit();
            }

               // handle exception if error in establishing connection
            catch (Exception error)
            {
                MessageBox.Show(error.ToString());
            }

        } // end method RunClient
    }
}
