package main

import (
	"encoding/json"
	"fmt"
	"time"

	"github.com/gorilla/websocket"
)

const (
	pingWrite = 30 * time.Second
	pongRead  = 30 * time.Second
	period    = pingWrite * 9 / 10
)

// Client represents a connection open to a channel.
type Client struct {
	Handle  string
	Channel *Hub
	Socket  *websocket.Conn
}

// ReceivedMessage is used for JSON interface.
type ReceivedMessage struct {
	Message string `json:"message"`
	Handle  string `json:"handle"`
}

// SentMessage is the json format used for sending the received message to all connected clients.
type SentMessage struct {
	Message string `json:"message"`
	HTML    string `json:"HTML"`
	Handle  string `json:"handle"`
}

// PingClient pings the client.
func (c *Client) PingClient() {

	for range time.Tick(period) {
		c.Socket.SetWriteDeadline(time.Now().Add(pingWrite))
		if err := c.Socket.WriteMessage(websocket.PingMessage, nil); err != nil {
			return
		}
	}

}

// ReadMessage listen for messages sent by the client's websocket.
func (c *Client) ReadMessage() {
	var SMessage ReceivedMessage
	for {
		err := c.Socket.ReadJSON(&SMessage)
		if err != nil {
			fmt.Printf("err: %v\n", err)
			// User's connection dropped. Remove the user from the hub's list.
			c.Channel.RemoveClient(c)

			break

		}

		c.Channel.JSONHandler(c, &SMessage)

	}

}

// WriteJSON sends the JSON to client's websocket.
func (c *Client) WriteJSON(v ...interface{}) {
	JSON, _ := json.Marshal(v)
	c.Socket.WriteMessage(1, []byte(JSON))
	//c.Socket.WriteJSON(v)

}

// SendMessage is an easier way of sending a message to the user.
func (c *Client) SendMessage(Value string) {
	c.Socket.WriteJSON(SentMessage{
		Handle:  "",
		HTML:    "",
		Message: Value,
	})

}
