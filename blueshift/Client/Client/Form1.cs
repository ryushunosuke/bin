using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
enum StatusCode // Server Status Codes.
{
    MessageReceived,
    UpdateListRemove,
    UpdateListAdd,
    PrivateMessage,
    RegistrationRequest,
    BadRequest,
    Unauthorized,
    BulkUpdateList,
    ServerName,
    MOTD,
    FriendshipRequest,
    FriendshipEnd,
    FriendshipList,
    Message,
    FriendshipReject,
}
namespace Blueshift_Client
{

    public partial class Form1 : Form
    {
        BindingList<string> ClientUsersList = new BindingList<string>(); // List for storing the online users.
        bool Terminating = false;
        bool Connected = false;
        Socket ClientSocket;
        bool Authenticated = false; // Used for checking if authentication worked.
        string ClientUserName = ""; // Client username.
        string ServerName;
        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormClosing += new FormClosingEventHandler(ApplicationExiting);
            InitializeComponent();
            UsersList.DataSource = ClientUsersList;
        }

        private StatusCode StatusCodeHandler(string MessageStatusCode) // Sorted half-assedly.
        {
            switch (MessageStatusCode)
            {
                case "BS200": // Status Code for message received.
                    return StatusCode.MessageReceived;
                case "BS211": // Status Code for a user connected.
                    return StatusCode.UpdateListAdd;
                case "BS212": // Status Code for a user disconnected.
                    return StatusCode.UpdateListRemove;
                case "BS214": // Stauts Code for friendship request.
                    return StatusCode.FriendshipRequest;
                case "BS216": // Status Code for messaging a user.
                    return StatusCode.Message;
                case "BS400": // Status Code for unrecognized request.
                    return StatusCode.BadRequest;
                case "BS401": // Status Code for unauthorized request.
                    return StatusCode.Unauthorized;
                case "BS213": // Status Code for bulk updating the online list.
                    return StatusCode.BulkUpdateList;
                case "BS414": // Status Code for removal of a friend.
                    return StatusCode.FriendshipEnd;
                case "BS000": // Status Code for the server name.
                    return StatusCode.ServerName;
                case "BS100": // Status Code for the MOTD.
                    return StatusCode.MOTD;
                default: // If the status code were none of those, return BadRequest.
                    return StatusCode.BadRequest;
            }
        }

        private void ApplicationExiting(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Application was give close signal.
            Connected = false;
            Terminating = true;
            Environment.Exit(0);
        }


        private void ClientConnectButton_Click(object sender, EventArgs e)
        {
            if (ClientConnectButton.Text == "Connect") // If not connected, connect.
            {
                ClientInputBox.Enabled = true;
                ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); // New socket.
                string IP = IPBox.Text;
                int PortNum;
                if (Int32.TryParse(PortBox.Text, out PortNum))
                {
                    try
                    {
                        ClientSocket.Connect(IP, PortNum); // Connect to the written server.

                        Connected = true;
                        ClientLogBox.AppendText("Connected to the server !\n");
                        ClientInputBox.Enabled = true;
                        Thread receieveThread = new Thread(Receieve);
                        receieveThread.Start();
                        ClientConnectButton.Text = "Disconnect";
                        ClientUsersList.Clear();
                        Authenticated = false;
                        ClientUserName = "";
                    }
                    catch
                    {
                        ClientLogBox.AppendText("Could not connect to the given server.\n");
                    }
                }
                else
                {
                    ClientLogBox.AppendText("Problem with the port number!\n");
                }
            }
            else if (ClientConnectButton.Text == "Disconnect") //If connected, disconnect.
            {
                Terminating = true;
                Connected = false;
                ClientSocket.Close();
                this.Text = "Disconnected from the server.";
                ClientConnectButton.Text = "Connect";
                ClientInputBox.Enabled = false;
                ClientLogBox.AppendText("Disconnected from the server.\n");
            }
        }

        private void BlueshiftStatusCodeHandler(string ReceivedCode, string Message)
        {
            switch (StatusCodeHandler(ReceivedCode))
            {
                case StatusCode.MessageReceived: // Message Received

                    string[] Messages = Message.Split(new string[] { "BS200 " }, StringSplitOptions.None);
                    foreach (string text in Messages)
                    {
                        ClientLogBox.AppendText(text + '\n');
                    }
                    break;
                case StatusCode.UpdateListAdd: // A user has connected
                    UpdateHandleList(Message, true, false);
                    break;
                case StatusCode.UpdateListRemove: // A user has disconnected
                    UpdateHandleList(Message, false, false);
                    break;
                case StatusCode.MOTD: // Server message of the day
                    ClientLogBox.AppendText(Message.Substring(0, Message.IndexOf("BS", 5) - 7) + '\n');
                    ServerName = Message.Substring(Message.IndexOf("BS000") + 6).Replace("\0", string.Empty);
                    this.Text = " Connected to " + ServerName + " :" + PortBox.Text;
                    break;
                case StatusCode.ServerName: // Name of the connected server
                    this.Text = " Connected to " + Message + " :" + PortBox.Text;
                    break;
                case StatusCode.Unauthorized: // Authorization has failed.
                    ClientLogBox.AppendText("Problem authenticating. Did you write your handle name correctly?\n");
                    Terminating = true;
                    Connected = false;
                    ClientConnectButton.Text = "Connect";
                    ClientInputBox.Enabled = false;
                    ClientSocket.Close();
                    break;
                case StatusCode.BulkUpdateList: // Bulk updating online list.
                    UpdateHandleList(Message, true, true);
                    break;
                case StatusCode.BadRequest: // Request unrecognized by server.
                    ClientLogBox.AppendText("A problem occurred. Disconnecting from server.\n");
                    Terminating = true;
                    Connected = false;
                    break;

            }
        }
        private void CommandHandler(string message)
        {
            string Command;
            string Message;
            if (message.IndexOf(" ") != -1)
            {
                Command = message.Substring(0, message.IndexOf(" ")); // Entered command had an argument attached.
                Message = message.Substring(message.IndexOf(" ")+1); // Arguments of the command
            }
            else
            {
                Command = message; // Entered command had no arguments to it
                Message = string.Empty;
            }
            
            switch (Command){
                case "/friend": // Make some friends
                    SendMessage("BS214 " + Message);
                    break;
                case "/unfriend": // Lose some friends
                    SendMessage("BS414 " + Message);
                    break;
                case "/msg": // Message your friends or strangers on the internet.
                    SendMessage("BS216 " + Message);
                    break;
                case "/friendlist": // A list of your friends.
                    SendMessage("BS217 ");
                    break;
                case "/reject":
                    SendMessage("BS218 " + Message);
                    break;
                default:
                    ClientLogBox.AppendText(Command + " unrecognized command." + '\n'); // Unrecognized command.
                    break;

            }
        }

        private void UpdateHandleList(string ReceivedHandle, bool Insert, bool Silent)
        {

            string[] UserNames = ReceivedHandle.Split(new string[] { "BS213 " }, StringSplitOptions.None);
            foreach (string HandleName in UserNames)
            {
                if (Insert) // A user has connected
                {
                    ClientUsersList.Add(HandleName);
                    if (!Silent) ClientLogBox.AppendText(HandleName + " has connected.\n");
                }
                else // A user has disconnected
                {
                    ClientUsersList.Remove(HandleName);
                    if (!Silent) ClientLogBox.AppendText(HandleName + " has disconnected.\n");
                }
            }
        }
        // Function for receiving packets and handling them.
        private void Receieve()
        {
            try
            {
                while (Connected)
                {
                    Byte[] Buffer = new Byte[1048576];
                    ClientSocket.Receive(Buffer); // Receive message.
                    string IncomingMessage = Encoding.Default.GetString(Buffer);
                    IncomingMessage = IncomingMessage.Substring(0, IncomingMessage.IndexOf("\0"));
                    if (IncomingMessage.Length >= 2 && IncomingMessage.Substring(0, 2) == "BS")
                        BlueshiftStatusCodeHandler(IncomingMessage.Substring(0,5),IncomingMessage.Substring(6)); // Send the message to Status Code Handler.
                    else if ((ClientUserName == "") && IncomingMessage.Contains("is either not in the database or already connected.\n"))
                    { // If the user has just joined and did not manage to authorize, cut the connection.
                        ClientSocket.Close();
                        Terminating = true;
                        Connected = false;
                        Authenticated = false;
                    }

                }
            }
            catch
            {
                // If the server cut the connection
                if (!Terminating)
                {
                    ClientLogBox.AppendText("Disconnected from the server.\n");
                }
                ClientUsersList.Clear(); // No one is online, clear the list.
                ClientSocket.Close();
                Connected = false;
                ClientConnectButton.Enabled = true;
            }
        }

        // Function where the messages are prepared for being sent.
        private void MessageBoxKeyPressed(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter) // If the pressed key is enter
            {

                string message = ClientInputBox.Text.Replace("\n", string.Empty);
                if (message.Length < 1048576 && (message != ""))
                {

                    // Need to check if the message is a either a command or a simple message.
                    if (message[0] == '/')
                    {
                        CommandHandler(message);
                    }
                    else // The message is not a client command. Send as a regular message.
                    {
                        if (!Authenticated)
                        {
                            SendMessage(message);
                        }
                        else
                        {
                            SendMessage("BS200 " + ClientUserName + ": " + message);
                        }
                    }
                }
                ClientInputBox.Text = "";
            }

        }

        private void SendMessage(string message)
        {
            try
            {
                ClientSocket.Send(Encoding.Default.GetBytes(message)); // Send the message
                                                                       // If the client had yet to authenticate, this will be the authentication message.
                if (!Authenticated)
                {
                    ClientUserName = message;
                    this.Text = " Connected to " + ClientUserName + "@" + ServerName + ":" + PortBox.Text; // Eye candy that looks great.

                    Authenticated = true;
                }
            }
            // If a trouble occurs when sending the message, terminate the connection.
            catch
            {
                ClientLogBox.AppendText("The connection is closed.\n");
                Terminating = true;
                Connected = false;
                IPBox.Enabled = true;
                PortBox.Enabled = true;
                ClientConnectButton.Enabled = true;
                ClientSocket.Close();
            }
        }
    }
}
