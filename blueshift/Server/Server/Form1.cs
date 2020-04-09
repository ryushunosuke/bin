using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

// Server now handles packets that have status codes attached to them.
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
    FriendshipReject,
}

struct IRCUser
{
    public Socket UserSocket;
    public string HandleName;
};

struct Notification
{
    public string Sender;
    public string Receiver;
    public string Message;
    public StatusCode Status;
    public bool NeedsReply;
    public string Reply;


    
    public static bool operator ==(Notification n1, Notification n2)
    {
        if(n1.Message == n2.Message)
        {
            if(n1.Receiver == n2.Receiver)
            {
                if(n1.Sender == n2.Sender)
                {
                    if(n1.Status == n2.Status)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    public static bool operator !=(Notification n1, Notification n2)
    {
        return !(n1 == n2);

    }

}
struct Friendship
{
    public string Person1;
    public string Person2;
}

struct UserFriends
{
    public string HandleName;
    public List<string> Friends;
}

namespace Blueshift_Server
{
    public partial class Form1 : Form
    {
        Socket ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        List<IRCUser> IRCUserList = new List<IRCUser>(); // Online backbone user list.
        BindingList<string> ServerUsersList = new BindingList<string>(); // Online gui user list.
        List<Notification> NotificationsList = new List<Notification>(); // Notifications list.
        List<Friendship> FriendshipList = new List<Friendship>(); //  friends.
        List<UserFriends> CurrentRelations = new List<UserFriends>(); // List of current session's friends.
        List<Notification> RemoveList = new List<Notification>(); // List of notifications that need to be removed.
        // Message of the day
        string MOTD = "BS100 (  ___ \\ ( \\      |\\     /|(  ____ \\(  ____ \\|\\     /|\\__   __/(  ____ \\\\__   __/\n| (   ) )| (      | )   ( || (    \\/| (    \\/| )   ( |   ) (   | (    \\/   ) (   \n| (__/ / | |      | |   | || (__    | (_____ | (___) |   | |   | (__       | |   \n|  __(  | |      | |   | ||  __)   (_____  )|  ___  |   | |   |  __)      | |   \n| (  \\ \\ | |      | |   | || (            ) || (   ) |   | |   | (         | |   \n| )___) )| (____/\\| (___) || (____/\\/\\____) || )   ( |___) (___| )         | |   \n|/ \\___/ (_______/(_______)(_______/\\_______)|/     \\|\\_______/|/          )_(   \n\nWelcome to Blueshift basic IRC server. Please authenticate using your registered handle name.";
        string ServerName = "Blueshift";
        bool Terminating = false;
        bool Listening = false;

        public Form1()
        {

            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormClosing += new FormClosingEventHandler(ApplicationExiting);
            InitializeComponent();
            UsersList.DataSource = ServerUsersList;
            this.Text = ServerName;

        }
        private StatusCode StatusCodeHandler(string MessageStatusCode)
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
                case "BS414": // Status Code for friendship removal.
                    return StatusCode.FriendshipEnd;
                case "BS216": // Status Code for messaging a user.
                    return StatusCode.PrivateMessage;
                case "BS217": // Status Code for dumping user's friendlist.
                    return StatusCode.FriendshipList;
                case "BS218": // Status Code for rejecting someone's friendship.
                    return StatusCode.FriendshipReject;
                case "BS400": // Status Code for unrecognized request.
                    return StatusCode.BadRequest;
                case "BS401": // Status Code for unauthorized request.
                    return StatusCode.Unauthorized;
                case "BS213": // Status Code for bulk updating the online list.
                    return StatusCode.BulkUpdateList;
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
            Listening = false;
            Terminating = true;
            Environment.Exit(0);
        }

        // Where the written message is processed and then sent.
        private void MessageBoxKeyPressed(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                string message = ServerInputBox.Text.Replace("\n", string.Empty);
                if (message.Length < 1048576 && (message != ""))
                {
                    Byte[] buffer = Encoding.Default.GetBytes("BS200 Server: " + message);
                    foreach (IRCUser user in IRCUserList) // Send it to each user.
                    {
                        try
                        {
                            user.UserSocket.Send(buffer);
                        }
                        catch // Problem with a user, stopping listening.
                        {
                            ServerLogBox.AppendText("There is a problem with the connection.\n");
                            Listening = false;
                            IPBox.Enabled = true;
                            PortBox.Enabled = true;
                            ServerHostButton.Enabled = true;
                            ServerHostButton.Text = "Host";
                            ServerSocket.Close();
                        }
                    }
                    ServerLogBox.AppendText("Server: " + message + '\n');
                }
                ServerInputBox.Text = "";
            }

        }

        // Checks the validity of a user by if the user exists in the database or is currently connected.
        // Returns false if the user can't connect.
        private bool CheckUserValidity(string NameToCheck)
        {
            NameToCheck = NameToCheck.Replace("\0", String.Empty);
            bool UserExists = true;
            //string line;
            //System.IO.StreamReader file = new System.IO.StreamReader(@"Z:\downloads\user_db.txt");
            //while ((line = file.ReadLine()) != null) // Read the file and process it line by line
            //{
            //    if (NameToCheck == line) UserExists = true;
            //}

            //file.Close();


            bool AlreadyConnected = false;
            foreach (IRCUser user in IRCUserList) // Check if the user is already connected.
            {
                if (user.HandleName == NameToCheck) AlreadyConnected = true;
            }

            return (UserExists && !AlreadyConnected);
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            if (ServerHostButton.Text == "Host") // If not hosting, host
            {
                try
                {

                    Terminating = false;
                    Listening = true;
                    UsersList.DataSource = ServerUsersList;
                    int ServerPort;
                    IPAddress ServerIP;
                    if (Int32.TryParse(PortBox.Text, out ServerPort))
                    {

                        if (IPAddress.TryParse(IPBox.Text, out ServerIP))
                        {

                            IPEndPoint EndPoint = new IPEndPoint(ServerIP, ServerPort); // Establish listening point.
                            ServerSocket.Bind(EndPoint); // Bind the end point.
                            ServerSocket.Listen(50); // Start listening.
                            Listening = true;
                            ServerInputBox.Enabled = true;
                            ServerHostButton.Text = "Close";

                            Thread AcceptThread = new Thread(Accept); // Start accepting messages.
                            AcceptThread.Start();
                            this.Text = ServerName + "(" + IPBox.Text + ") Listening on port " + ServerPort;
                            ServerLogBox.AppendText("Started listening on Port: " + ServerPort + '\n');

                        }
                        else
                        {
                            ServerLogBox.AppendText("Problem with the IP number.\n");
                        }



                    }
                    else
                    {
                        ServerLogBox.AppendText("Problem with port number.\n");
                    }
                }
                catch
                {
                    ServerLogBox.AppendText("Problem establishing connection. Did you write the values correctly?\n");
                }
            }
            else if (ServerHostButton.Text == "Close") // If hosting, stop hosting.
            {
                Terminating = true;
                Listening = false;
                ServerHostButton.Text = "Host";
                ServerLogBox.AppendText("Closing all connections.\n");
                ServerSocket.Close();
                ServerUsersList.Clear(); // Remove everyone from the gui online user list.
                foreach (IRCUser user in IRCUserList) // Close all of the sockets.
                {
                    user.UserSocket.Close();
                }
                IRCUserList.Clear(); // Remove all of the, now offline, users.
                FriendshipList.Clear(); // No longer keeping ahold of the friendships.
                NotificationsList.Clear(); // No longer keeping ahold of the notifications.
                this.Text = ServerName + ": Disconnected"; // Bling bling
                ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); // New socket for server to listen to.

            }
        }
        private void Accept()
        {
            while (Listening)
            {
                try
                {
                    Byte[] Buffer = new Byte[1048576];
                    Socket NewClient = ServerSocket.Accept();
                    NewClient.Send(Encoding.Default.GetBytes(MOTD)); // Send the MOTD.
                    NewClient.Send(Encoding.Default.GetBytes("BS000 " + ServerName)); // Sends the configured ServerName.
                    string HandleName;
                    NewClient.Receive(Buffer); // Receieve handle name.
                    HandleName = Encoding.ASCII.GetString(Buffer);
                    HandleName = HandleName.Replace("\0", string.Empty);

                    if (!CheckUserValidity(HandleName))
                    {
                        Byte[] ErrorMessage = Encoding.Default.GetBytes("BS401 " + HandleName + " is either not in the database or already connected.\n");
                        NewClient.Send(ErrorMessage); // Send Unauthorized signal to the client.
                        NewClient.Close();
                    }
                    else
                    {
                        IRCUser User;
                        User.UserSocket = NewClient; // Newly joined user's socket.
                        User.HandleName = HandleName; // Newly Joined user's handle.
                        bool found = false; // Initialize user's friendlist.
                        foreach(UserFriends user in CurrentRelations)
                        {
                            if(user.HandleName == HandleName)
                            {
                                found = true;
                            }
                        }
                        if (!found) // User has no friends, nor a template.
                        {
                            CurrentRelations.Add(new UserFriends() {
                                HandleName = HandleName,
                                Friends = new List<string>()
                            });
                        }
                        User.UserSocket.Send(Encoding.Default.GetBytes("BS211 " + HandleName)); // Send itself the User update method to signal connection succeeded.
                        foreach (IRCUser user in IRCUserList)
                        {
                            user.UserSocket.Send(Encoding.Default.GetBytes("BS211 " + HandleName)); // Send Signal to every other client that someone has connected
                            User.UserSocket.Send(Encoding.Default.GetBytes("BS213 " + user.HandleName)); // Send the current list of connected users to newly connected user.
                        }
                        IRCUserList.Add(User);
                        ServerUsersList.Add(HandleName); // Add the online user to the online users list.
                        ServerLogBox.AppendText(HandleName + " has connected.\n"); // Append message to the server's gui log box.
                        Thread.Sleep(1000);

                        try
                        {
                            ServerLogBox.AppendText("Accessing the notifications list...\n");

                            foreach (Notification notification in NotificationsList) // The connected user might have a notification waiting for him.
                            {
                                if (notification.Receiver == User.HandleName)
                                {

                                    NotificationHandler(notification);
                                }
                            }
                        }
                        catch
                        {

                            foreach (Notification notification in NotificationsList) // The connected user might still have a notifications waiting for him.
                            {
                                if (notification.Receiver == User.HandleName && notification.Status==StatusCode.FriendshipRequest)
                                {
                                    NotificationHandler(notification);
                                }
                            }

                        }
                        RemovalListUpdated();
                        Thread ReceiveThread = new Thread(Receive);
                        ReceiveThread.Start();
                    }
                }
                catch
                {
                    if (Terminating)
                    {
                        Listening = false; // Closing server, stop listening.
                    }
                    else
                    {
                        ServerLogBox.AppendText("A client had a problem connecting.\n");
                    }
                }

            }
        }
        private void BlueshiftStatusCodeHandler(IRCUser User, string ReceivedCode, string Message)
        {
            IRCUser guca = User;
            switch (StatusCodeHandler(ReceivedCode))
            {
                case StatusCode.MessageReceived: // Message Received
                    foreach (IRCUser user in IRCUserList)
                    {
                        user.UserSocket.Send(Encoding.Default.GetBytes("BS200 " + Message)); // Send the message to everyone
                    }
                    ServerLogBox.AppendText(Message + '\n');
                    break;
                case StatusCode.FriendshipRequest:
                    if (Message.Replace(" ", string.Empty) == string.Empty)
                        break;
                    if (User.HandleName != Message)
                        FriendshipHandler(User, Message, true); // Semi-officially friends.
                    else
                        User.UserSocket.Send(Encoding.Default.GetBytes("BS200 You cannot become friends with yourself. Sorry."));
                    break;
                case StatusCode.FriendshipEnd:
                    FriendshipHandler(User, Message, false); // No longer friends.
                    break;
                case StatusCode.FriendshipList:
                    string List = "";
                    UserFriends UserFriend = FindFriendsOfUser(User.HandleName);
                    foreach (string friend in UserFriend.Friends)
                    {
                        List += friend + ", ";
                    }
                    if (UserFriend.Friends.Count != 0)
                        User.UserSocket.Send(Encoding.Default.GetBytes("BS200 Your friendlist consists of: " + List));
                    else
                        User.UserSocket.Send(Encoding.Default.GetBytes("BS200 You have no friends."));
                    break;
                case StatusCode.PrivateMessage:
                    if (Message.Split(';')[0] == "<all>")
                    {
                        foreach (UserFriends TestFriend in CurrentRelations)
                        {
                            if (TestFriend.HandleName == User.HandleName)
                            {
                                foreach (string friend in TestFriend.Friends)
                                {
                                    NotificationsList.Add(new Notification()
                                    {
                                        Message = Message.Split(';')[1],
                                        Receiver = friend,
                                        Sender = User.HandleName,
                                        NeedsReply = false,
                                        Status = StatusCode.PrivateMessage
                                    });
                                    NotificationsListUpdated();
                                }
                            }
                        }
                    }
                    else {
                        NotificationsList.Add(new Notification()
                        {
                            Message = Message.Split(';')[1],
                            Receiver = Message.Split(';')[0],
                            Sender = User.HandleName,
                            NeedsReply = false,
                            Status = StatusCode.PrivateMessage
                        });
                        NotificationsListUpdated();
                    }
                    break;
                case StatusCode.FriendshipReject:
                    try
                    {
                        foreach (Notification notif in NotificationsList)
                        {
                            if (notif.Receiver == User.HandleName &&
                                notif.Sender == Message &&
                                notif.Status == StatusCode.FriendshipRequest)
                            {
                                //NotificationsList.Remove(notif);
                                RemoveList.Add(notif);
                            }
                        }
                        RemovalListUpdated();
                    }
                    catch
                    {
                        ServerLogBox.AppendText("A notification was removed from the list.\n");
                    }
                    try
                    {
                        foreach(Friendship proposed in FriendshipList)
                        {
                            if ((proposed.Person1 == Message && proposed.Person2 == User.HandleName) || (proposed.Person1 == User.HandleName && proposed.Person2 == Message))
                                FriendshipList.Remove(proposed);
                        }
                    }
                    catch
                    {
                        ServerLogBox.AppendText("A friendship rejection occurred.\n");
                    }
                    break;

                case StatusCode.BadRequest:
                    ServerLogBox.AppendText("A bad request were made.\n");
                    break;
                default:
                    ServerLogBox.AppendText("An unidentifiable request were made.\n");
                    break;

            }
        }


        private UserFriends FindFriendsOfUser(string HandleName)
        {
            foreach (UserFriends user in CurrentRelations)
            {
                if(HandleName == user.HandleName)
                {
                    return user;
                }
            }
            return new UserFriends() { HandleName = "404" };
        }

        private void FriendshipHandler(IRCUser user, string Receiver, bool friend)
        {
            
            string Requester = user.HandleName;

            if (friend) // They want to be friends.
            {
                foreach (IRCUser User in IRCUserList)
                {
                    if (User.HandleName == Requester)
                    {
                        UserFriends UserFriend = FindFriendsOfUser(User.HandleName);

                        foreach (string PossibleFriend in UserFriend.Friends)
                        {
                            if (PossibleFriend == Receiver)
                            {
                                User.UserSocket.Send(Encoding.Default.GetBytes("BS200 You're already friends with " + Receiver + "."));
                                return;
                            }
                        }
                    }
                }

                // Needs a check to see if the person requesting the friendship already had a friendship request made by that person
                bool BothRequest = false;
                foreach (Friendship token in FriendshipList)
                {
                    if (token.Person2 == Requester && token.Person1 == Receiver)
                    {
                        BothRequest = true;
                    }
                }

                if (BothRequest)
                {
                    // They both want to be friends now. Have to remove the notification
                    try
                    {
                        foreach (Notification notif in NotificationsList)
                        {
                            if ((notif.Message == Requester + " wishes to be  friends with you. type /friend "
                                + Receiver + " to accept. If you wish to not friend them, you may type /reject "+ Requester +".")
                                || notif.Status == StatusCode.FriendshipRequest )
                            {
                                //NotificationsList.Remove(notif);
                                RemoveList.Add(notif);
                            }
                        }
                    }
                    catch
                    {
                        ServerLogBox.AppendText("A Notification has been removed.\n");// Nothing.
                    }
                    Notification notify = new Notification();
                    notify.Sender = Requester;
                    notify.Receiver = Receiver;
                    notify.Status = StatusCode.FriendshipRequest;
                    notify.Message = "You are now friends with " + notify.Sender;
                    NotificationsList.Add(notify);
                    NotificationsListUpdated();
                    notify.Receiver = Requester;
                    notify.Message = "You are now friends with " + Receiver;
                    NotificationsList.Add(notify);
                    NotificationsListUpdated();
                    foreach (IRCUser User in IRCUserList)
                    {
                        if (User.HandleName == Receiver)
                        {
                            //User.FriendList.Add(Requester); // Officially friends.
                            foreach(UserFriends UserFriend in CurrentRelations)
                            {
                                if(User.HandleName == UserFriend.HandleName)
                                {
                                    UserFriend.Friends.Add(Requester);
                                }
                            }
                        }
                        else if (User.HandleName == Requester)
                        {
                            //User.FriendList.Add(Receiver);
                            foreach (UserFriends UserFriend in CurrentRelations)
                            {
                                if (User.HandleName == UserFriend.HandleName)
                                {
                                    UserFriend.Friends.Add(Receiver);
                                }
                            }
                        }
                    }
                    try
                    {
                        foreach (Friendship token in FriendshipList)
                        {
                            if (token.Person2 == Requester && token.Person1 == Receiver)
                            {
                                FriendshipList.Remove(token);
                            }
                        }
                    }
                    catch
                    {
                    }


                }
                else
                {
                    Notification notify = new Notification();
                    notify.Sender = Requester;
                    notify.Receiver = Receiver;
                    notify.Status = StatusCode.FriendshipRequest;
                    notify.NeedsReply = true;
                    notify.Message = notify.Sender + " wishes to be  friends with you. type /friend " + notify.Sender + " to accept. If you wish to not friend them, you may type /reject " + notify.Sender + ".";
                    foreach (Notification notification in NotificationsList)
                    {
                        if(notify == notification)
                        {
                            return;
                        }
                    }
                    Friendship friends = new Friendship();
                    friends.Person1 = Requester;
                    friends.Person2 = Receiver;
                    FriendshipList.Add(friends);
                    NotificationsList.Add(notify);
                    NotificationsListUpdated();
                }

            }
            else // No longer want to be friends
            {
                UserFriends UserFriend = FindFriendsOfUser(user.HandleName);
                if (UserFriend.Friends.IndexOf(Receiver) == -1){ // Meaning they are not friends
                    NotificationsList.Add(new Notification()
                    {
                        Message = "You and " + Receiver + " are not friends.",
                        Receiver = user.HandleName,
                        Sender = "Server",
                        Status = StatusCode.FriendshipEnd
                    });
                    NotificationsListUpdated();
                    return;
                }
                try
                {
                    UserFriend.Friends.Remove(Receiver);
                    try
                    {
                        foreach (UserFriends OnlineUser in CurrentRelations)
                        {
                            if (Receiver == OnlineUser.HandleName)
                                OnlineUser.Friends.Remove(user.HandleName);
                        }
                    }
                    catch
                    {
                        ServerLogBox.AppendText("A friendship just ended.\n");
                    }

                    NotificationsList.Add(new Notification()
                    {
                        Message = "You and " + user.HandleName + " are no longer friends.",
                        Receiver = Receiver,
                        Sender = user.HandleName,
                        Status = StatusCode.FriendshipEnd
                    });
                    NotificationsListUpdated();
                }
                catch
                {
                    ServerLogBox.AppendText("Item removed from List while access were still had\n");
                }
            }

        }

        private void NotificationsListUpdated()
        {
            if (NotificationsList.Count != 0)
            {
                try
                {

                        NotificationHandler(NotificationsList[NotificationsList.Count-1]);
                    
                }
                catch
                {
                    ServerLogBox.AppendText("A notification has been handled.\n");
                }
            }
        }

        private void NotificationHandler(Notification notification)
        {

            bool UserOnline = false;
            switch (notification.Status)
            {
                case StatusCode.FriendshipRequest:
                    foreach (IRCUser user in IRCUserList)
                    {
                        if (user.HandleName == notification.Receiver)
                        {
                            user.UserSocket.Send(Encoding.Default.GetBytes("BS200 " + notification.Message));
                            UserOnline = true;
                        }
                    }
                    if (UserOnline) // Meaning they've been sent the notification.
                    {
                        if (!notification.NeedsReply)
                        {
                            RemoveList.Add(notification);
                            //RemovalListUpdated();

                            //NotificationsList.Remove(notification);
                        }
                    }
                    break;
                case StatusCode.PrivateMessage:
                    foreach (IRCUser user in IRCUserList)
                    {
                        if (user.HandleName == notification.Receiver)
                        {
                            user.UserSocket.Send(Encoding.Default.GetBytes("BS200 <*> " + notification.Sender + ": " +  notification.Message));
                            UserOnline = true;
                        }
                    }
                    if (UserOnline) // Meaning they've been sent the notification.
                    {
                        RemoveList.Add(notification);
                        //NotificationsList.Remove(notification);
                    }
                    break;
                case StatusCode.FriendshipEnd:
                    foreach (IRCUser user in IRCUserList)
                    {
                        if (user.HandleName == notification.Receiver)
                        {
                            user.UserSocket.Send(Encoding.Default.GetBytes("BS200 " + notification.Message));
                            UserOnline = true;
                        }
                    }
                    if (UserOnline)
                        RemoveList.Add(notification);
                        //NotificationsList.Remove(notification);
                    break;

            }
            //RemovalListUpdated();
        }

        private void RemovalListUpdated()
        {
            if (RemoveList.Count > 0)
            {
                foreach (Notification notification in RemoveList)
                {
                    NotificationsList.Remove(notification);
                }
            }
            RemoveList.Clear();
        }

        // Receiving messages.
        private void Receive()
        {
            IRCUser ThisUser = IRCUserList[IRCUserList.Count() - 1];
            bool Connected = true;
            while (Connected && !Terminating)
            {
                try
                {
                    Byte[] Buffer = new Byte[1048576];
                    ThisUser.UserSocket.Receive(Buffer); // Receive the message from client
                    string IncomingMessage = Encoding.Default.GetString(Buffer);
                    IncomingMessage = IncomingMessage.Substring(0, IncomingMessage.IndexOf("\0")); // Get the meat of the message
                    if (IncomingMessage.Length >= 6 && IncomingMessage.Substring(0, 2) == "BS")
                        BlueshiftStatusCodeHandler(ThisUser, IncomingMessage.Substring(0, 5), IncomingMessage.Substring(6)); // Send the message to Status Code Handler.
                    else
                    {
                        ServerLogBox.AppendText("Recieved a bad request.\n");
                    }

                }
                catch
                {
                    // Client's connection has closed
                    do
                    {
                        try { 
                            foreach (Notification notification in NotificationsList) // Clear notification queue of read notifications.
                            {
                                if (notification.Receiver == ThisUser.HandleName)
                                {
                                    if (!notification.NeedsReply)
                                    {
                                        RemoveList.Add(notification);
                                        RemovalListUpdated();
                                    }
                                    
                                }
                            }
                        }
                        catch
                        {
                            ServerLogBox.AppendText("List count:"+RemoveList.Count+'\n');
                        }
                    } while (RemoveList.Count > 0);
                    if (!Terminating) ServerLogBox.AppendText(ThisUser.HandleName + " has disconnected.\n");
                    ThisUser.UserSocket.Close();
                    IRCUserList.Remove(ThisUser); // Remove the user from the backbone user list
                    ServerUsersList.Remove(ThisUser.HandleName); // Remove the user from the gui list.
                    Connected = false;
                    foreach (IRCUser user in IRCUserList) // Send all the other users signal that the this user has disconnected
                    {
                        user.UserSocket.Send(Encoding.Default.GetBytes("BS212 " + ThisUser.HandleName));
                    }
                }
            }
        }

    }
}
class CONSTS { int MessageLength = 10248576; };
