package main

import (
	"net/http"
)

func serve(w http.ResponseWriter, r *http.Request) {
	http.ServeFile(w, r, "./folder")
}

func main() {
	fs := http.FileServer(http.Dir("./folder"))
	// http.HandleFunc("/", serve)
	http.Handle("/", fs)
	http.ListenAndServe(":8080", nil)
}
