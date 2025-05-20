This solution has four projects concerned with the game of Go
1) Go
   Stand alone C# program for practicing Go. It shows the board, can save and read SGF go files, detect captures and Kos,
   copies and pastes.
   It can reference (use) GoCarta.dll and interact with GoServer.
   It has functioning C# version of Carta's algorithm mostly in ScoreCarta.cs
3) GoCarta
   Contains VB source code by Andy Carta in modScore.vb (copied fom https://github.com/Fantasio1960/Computing-Go-Scoring)
   and the first attempt at translating it to C# in modScore.cs, mostly by CoPilot. This ended up in ScoreCarta.cs.
   This project is no longer used.
4) GoCartaDLL
   Contains modScore.vb very similar to that in GoCarta which builds to a DLL which can be used by the Go solution
   to check that 1) the code from Carta does what he says (it does!) and 2) compare its results to those from
   the C# version in ScoreCarta.cs. They now agree for all 623 Dyer files. The DLL is no longer used.
5) GoServer
   This contains the SignalR C# code that will be on the server. Currently (2025-05-20) it only contains some test code
   to check that the connection to the server can work. (It can)
   
Carta paper is at https://www.uni-trier.de/fileadmin/fb4/prof/BWL/FIN/Veranstaltungen/A_static_method_for_computing_the_score_of_a_Go_game__Carta_.pdf
