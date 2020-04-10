package main

import "strings"

// Hub is a channel. It has a channel identifier (name) and a list of client connections.
type Hub struct {
	Identifier string
	Clients    []*Client
}

// BroadcastMessage sends the argument message to all of the clients in the channel.
func (h *Hub) BroadcastMessage(JSON interface{}) {

	for _, client := range h.Clients {
		client.WriteJSON(JSON)
	}

}

// RemoveClient removes given client from hub's client slice.
func (h *Hub) RemoveClient(c *Client) {
	for i, el := range h.Clients {
		if el == c {
			h.Clients = append(h.Clients[:i], h.Clients[i+1:]...)
		}
	}

}

// JSONHandler handles the taken JSON from a client.
func (h *Hub) JSONHandler(c *Client, SMessage *ReceivedMessage) {
	switch SMessage.Message[0] {
	case 47: // '/'
		h.CommandHandler(c, SMessage)
		return
	case 92: // '\'
		SMessage.Message = SMessage.Message[1:]
	default:
		break

	}

	h.BroadcastMessage(SentMessage{
		Message: SMessage.Message,
		Handle:  c.Handle,
	})

}

// CommandHandler handles messages that start with /
func (h *Hub) CommandHandler(c *Client, Message *ReceivedMessage) {
	ss := strings.SplitAfterN(Message.Message, " ", 2)
	if len(ss) < 1 {
		return
	}
	switch ss[0] {
	case "/auth":

	default:
		c.SendMessage(`Invalid Command. Did you mean to start your message with a '/'? Try starting with a "\\/"`)
	}

}
