package main

import (
	"fmt"
	"log"
	"net/http"
	"strings"
	"time"

	"github.com/gorilla/mux"
	"github.com/gorilla/websocket"
)

// MServer is where all the channels that hold all the clients are held.
var MServer = Server{
	Hubs: make(map[string]*Hub),
}

// Server is the main part that handles the websocket and then directs the socket to the proper channel.
type Server struct {
	Hubs map[string]*Hub
}

//WSHandler handles connections made by http://domain/ws*
func WSHandler(w http.ResponseWriter, r *http.Request) {
	if strings.Count(r.URL.Path, "3") > 2 {
		return
	}
	params := mux.Vars(r)
	channel := params["channel"]
	handle := params["handle"]
	hub, ok := MServer.Hubs[channel]
	if !ok { // No chat existed for such a channel
		hub = &Hub{
			Clients:    make([]*Client, 0),
			Identifier: channel,
		}
		MServer.Hubs[channel] = hub

	}
	client := &Client{
		Socket:  AcceptWS(w, r),
		Channel: hub,
		Handle:  handle,
	}
	client.Socket.SetReadDeadline(time.Now().Add(pongRead))
	client.Socket.SetPongHandler(func(string) error { client.Socket.SetReadDeadline(time.Now().Add(pongRead)); return nil })
	go client.PingClient()
	if client.Channel == nil || client.Socket == nil {
		return
	}
	hub.Clients = append(hub.Clients, client)

	fmt.Printf("%v, channel: %v, handle: %v\n", r.URL.Path, channel, handle)
	fmt.Printf("%v", hub.Clients)

	go client.ReadMessage()

}

// AcceptWS accepts a websocket connection.
func AcceptWS(w http.ResponseWriter, r *http.Request) *websocket.Conn {
	conn, err := upgrader.Upgrade(w, r, nil)
	if err != nil {
		log.Println(err)
		return nil
	}
	return conn

}

var upgrader = websocket.Upgrader{
	ReadBufferSize:  2048,
	WriteBufferSize: 2048,
}

func main() {
	r := mux.NewRouter()
	r.PathPrefix("/ws/{channel}/{handle}").HandlerFunc(WSHandler)
	r.PathPrefix("/ws/{channel}").HandlerFunc(WSHandler)

	srv := &http.Server{
		Handler:      r,
		Addr:         "localhost:24791",
		WriteTimeout: 15 * time.Second,
		ReadTimeout:  15 * time.Second,
	}
	log.Fatal(srv.ListenAndServe())
}
