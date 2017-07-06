using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace KhadgarConsole
{
    class KhadgarConsole
    {
        private static byte[] _data;
        // Enter in channel (the username of the stream chat you wish to connect to) without the #
        private const string _channel = "ciarenni";
        private static string _messageShell;
        private static NetworkStream _stream;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            _messageShell  = ":" + _channel + "!" + _channel + "@" + _channel + ".tmi.twitch.tv PRIVMSG #" + _channel + " :{0}\r\n";
            Int32 port = 6667;
            TcpClient client = new TcpClient("irc.twitch.tv", port);

            // Get a client stream for reading and writing.
            //  Stream stream = client.GetStream();
            _stream = client.GetStream();

            //grab the bot's login info from an xml file so sensitive info isn't publically posted
            var xmlLoginInfo = XDocument.Load(@"..\..\..\KhadgarBot\Resources\loginInfo.xml");
            var root = xmlLoginInfo.Descendants("root");
            var botNickname = root.Descendants("nick").First().Value;
            var botPass = root.Descendants("pass").First().Value;

            // Send the message to the connected TcpServer.
            string loginstring = string.Format("PASS oauth:{0}\r\nNICK {1}\r\n", botPass, botNickname);
            Byte[] login = System.Text.Encoding.ASCII.GetBytes(loginstring);
            _stream.Write(login, 0, login.Length);
            Console.WriteLine("Sent login.\r\n");
            Console.WriteLine(loginstring);

            // Receive the TcpServer.response.
            // Buffer to store the response bytes.
            _data = new Byte[512];

            // String to store the response ASCII representation.
            String responseData = String.Empty;

            // Read the first batch of the TcpServer response bytes.
            Int32 bytes = _stream.Read(_data, 0, _data.Length);
            responseData = System.Text.Encoding.ASCII.GetString(_data, 0, bytes);
            Console.WriteLine("Received WELCOME: \r\n\r\n{0}", responseData);

            // send message to join channel

            string joinstring = "JOIN " + "#" + _channel + "\r\n";
            Byte[] join = System.Text.Encoding.ASCII.GetBytes(joinstring);
            _stream.Write(join, 0, join.Length);
            Console.WriteLine("Sent channel join.\r\n");
            Console.WriteLine(joinstring);

            //SetupRequests();

            // PMs the channel to announce that it's joined and listening
            // These three lines are the example for how to send something to the channel
            //:khadgarbot!khadgarbot@khadgarbot.tmi.twitch.tv JOIN #ciarenni
            //:khadgarbot.tmi.twitch.tv 353 khadgarbot = #ciarenni :khadgarbot
            //:khadgarbot.tmi.twitch.tv 366 khadgarbot #ciarenni :End of /NAMES list

            // Lets you know its working
            Console.WriteLine("TWITCH CHAT HAS BEGUN.\r\n");
            Console.WriteLine("BE CAREFUL.\r\n\r\n");

            //SendMessageToChat("KhadgarBot has arrived.");

            bool hasJoined = false;

            while (true)
            {

                // build a buffer to read the incoming TCP stream to, convert to a string

                byte[] myReadBuffer = new byte[1024];
                StringBuilder myCompleteMessage = new StringBuilder();
                int numberOfBytesRead = 0;

                // Incoming message may be larger than the buffer size.
                do
                {
                    try
                    {
                        numberOfBytesRead = _stream.Read(myReadBuffer, 0, myReadBuffer.Length);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Something went wrong with reading incoming data.\r\n", e);
                    }

                    myCompleteMessage.AppendFormat("{0}", Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead));
                }

                // when we've received data, do Things

                while (_stream.DataAvailable);

                // Print out the received message to the console.
                Console.WriteLine(myCompleteMessage.ToString());
                switch (myCompleteMessage.ToString())
                {
                    // Every 5 minutes the Twitch server will send a PING, this is to respond with a PONG to keepalive

                    case "PING :tmi.twitch.tv\r\n":
                        try
                        {
                            Byte[] say = System.Text.Encoding.ASCII.GetBytes("PONG :tmi.twitch.tv\r\n");
                            _stream.Write(say, 0, say.Length);
                            Console.WriteLine("Ping? Pong!");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Ping pong failed.\r\n", e);
                        }
                        break;

                    // If it's not a ping, it's probably something we care about.  Try to parse it for a message.
                    default:
                        try
                        {
                            string messageParser = myCompleteMessage.ToString();
                            string[] messageArray = messageParser.Split(':');
                            string[] preamble = messageArray[1].Split(' ');
                            string tochat;

                            // This means it's a message to the channel.  Yes, PRIVMSG is IRC for messaging a channel too
                            if (preamble[1] == "PRIVMSG")
                            {
                                string[] sendingUser = preamble[0].Split('!');
                                string message = messageArray[2].Trim();
                                tochat = sendingUser[0] + ": " + messageArray[2];

                                // sometimes the carriage returns get lost (??)
                                if (tochat.Contains("\n") == false)
                                {
                                    tochat = tochat + "\n";
                                }

                                // Ignore some well known bots
                                if (sendingUser[0] != "moobot" && sendingUser[0] != "whale_bot")
                                {
                                    //this seems to parrot back what the user says, so it remains commented out
                                    //SendKeys.SendWait(tochat.TrimEnd('\n'));
                                    switch(message)
                                    {
                                        case "Khadgar?": SendMessageToChat("Yes, mage?");
                                            break;
                                        case "!apexis": SendMessageToChat("Collect 4986 crystals for me. Don't look at me like that. I'm a mage. I did the math. I need exactly 4986 crystals. 4985 is inadequate. 4987 is of course absurd. Four thousand, nine hundred eighty-six. Go!");
                                            break;
                                        case "set color": SendMessageToChat("/color DodgerBlue");
                                            break;
                                    }

                                }
                            }
                            // A user joined.
                            else if (preamble[1] == "JOIN")
                            {
                                //string[] sendingUser = preamble[0].Split('!');
                                //tochat = "USER JOINED: " + sendingUser[0];
                                Console.WriteLine("Successfully joined channel: " + _channel);
                                //SetupRequests();
                                //SendMessageToChat(tochat.TrimEnd('\n'));
                                //SendKeys.SendWait(tochat.TrimEnd('\n'));
                            }
                            else if (Regex.IsMatch(preamble[1],@"\d\d\d"))
                            {
                                var messageCode = Int32.Parse(preamble[1]);
                                switch(messageCode)
                                {
                                    case 353: SetupRequests();
                                        break;
                                }
                            }
                        }
                        // This is a disgusting catch for something going wrong that keeps it all running.  I'm sorry.
                        catch (Exception e)
                        {
                            Console.WriteLine("Something went wrong in the main loop.\r\n", e);
                        }

                        // Uncomment the following for raw message output for debugging
                        //
                        //Console.WriteLine("Raw output: " + message[0] + "::" + message[1] + "::" + message[2]);
                        //Console.WriteLine("You received the following message : " + myCompleteMessage);
                        break;
                }
            }

            // Close everything.  Should never happen because you gotta close the window.
            _stream.Close();
            client.Close();
            Console.WriteLine("\n Press Enter to continue...");
            Console.Read();


        }

        private static void SendMessageToChat(string message)
        {
            //string.Format(_messageShell, message);
            Byte[] announce = System.Text.Encoding.ASCII.GetBytes(string.Format(_messageShell, message));
            _stream.Write(announce, 0, announce.Length);
        }

        private static void SetupRequests()
        {
            Byte[] setupString = System.Text.Encoding.ASCII.GetBytes("CAP REQ :twitch.tv/tags twitch.tv/commands twitch.tv/membership");
            _stream.Write(setupString, 0, setupString.Length);
            Console.WriteLine("Sent setup requests.");

            //byte[] myReadBuffer = new byte[1024];
            //StringBuilder myCompleteMessage = new StringBuilder();
            //int numberOfBytesRead = 0;

            // Incoming message may be larger than the buffer size.
            //do
            //{
            //    try
            //    {
            //        numberOfBytesRead = _stream.Read(myReadBuffer, 0, myReadBuffer.Length);
            //    }
            //    catch (Exception e)
            //    {
            //        Console.WriteLine("Something went wrong with reading incoming data.\r\n", e);
            //    }

            //    myCompleteMessage.AppendFormat("{0}", Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead));
            //}

            //// when we've received data, do Things

            //while (_stream.DataAvailable);

            //// Print out the received message to the console.
            //Console.WriteLine(myCompleteMessage.ToString()+"\r\n");
        }
    }
}