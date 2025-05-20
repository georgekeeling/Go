' how to use this module:
' the routine "ScoreCompute" must be called with the following parameters:
' - NN1 = goban's dimension - 1 (usually 18)
' - japanese = true when territory scoring, = false when area scoring
' - komi = komi's value
' - hc = number of handicap stones (0 = no handicap)
' - whitemoves = number of white moves played
' - blackmoves = number of black moves played
' also, the module exposes the integer matrix "position" (18 x 18) that must be filled in advance according to the following rules:
' 0 = empty intersection; 1 = black stone; -1 = white stone; it may be filled manually or by means of an SGF file
' the matrix may be used by the main program in order to show the position whose score is computed and so on
' the module returns an integer value according to the following rules:
' 0 = jigo (draw)
' +n = black wins by n
' -n = white wins by n
' 1000 = black wins by resignation (= black is leading, and dame are > NN1*NN1/6 (usually 54)
' -1000 = white wins by resignation (= white is leading, and dame are > NN1*NN1/6 (usually 54)
' if compiled with the global variable GUI the module has GUI capabilities:
' it calls an external routine called "draw", that can draw something over an image of the goban, with the following parameters:
' - "element" (0 = stone; 1 = mark on a stone; 2 = point of territory; 3 = point of forced connection; 4 = stone dead, thus virtually removed )
' - coordinates on the goban (x, y, usually between 0 and 18)
' - color of "element"
' it calls an external routine called "debug", that displays (usually on a textBox) some informations about the strings
' (that happens if the main program calls the public routine "stringAnalysis" specifying the number of the string plus the parameter "true":
' the number of the string is kept in the public matrix ID(), indexed by means of the goban's coordinates, i.e. ID(9,9) contains the number
' of the string including the stone (if there is one) on the Tengen point)
' it calls an external routine called "score", that displays (usually on a textBox) extended informationss regarding the score of the game

Public Module modScore

    Public Const stone As Integer = 0
    Public Const ministone As Integer = 1
    Public Const territory As Integer = 2
    Public Const connection As Integer = 3
    Public Const removing As Integer = 4
    Public board(18, 18) As Integer
    Public ID(18, 18) As Integer
    Public str As Integer
    Public shows As Boolean
    Public N1 As Integer
    Public GroupNumber As Integer
    Public StringNumber As Integer
    Public rs As String

    Public Structure chain
        Dim id As Integer
        Dim size As Integer
        Dim colour As Integer
        Dim p() As Point
        Dim status As Integer
        Dim liberty As ArrayList
        Dim liberties As Double
        Dim eyes As ArrayList
        Dim eyesnumber As Integer
        Dim eyelikes As ArrayList
        Dim eyelikesnumber As Integer
        Dim specialEyes As ArrayList
        Dim specialEyesNumber As Integer
        Dim group As Integer
        Dim groupTerritory As Integer
        Dim emptyNeighbourPoints As ArrayList
        Dim neighbourPoints As ArrayList
    End Structure

    Public Structure group
        Dim id As Integer
        Dim size As Integer
        Dim element() As Integer
        Dim stones As Integer
        Dim colour As Integer
        Dim eyes As ArrayList
        Dim eyesnumber As Integer
        Dim eyelikes As ArrayList
        Dim specialEyes As ArrayList
        Dim territory As Integer
        Dim liberty As ArrayList
    End Structure

    Public Chains(100) As chain
    Public groups(70) As group
    Public controlled(18, 18) As Boolean
    Dim whiteArea As Double
    Dim blackArea As Double
    Dim backup_board(18, 18) As Integer
    Dim backup_intensity(18, 18) As Integer
    Dim bonus(100) As Boolean
    Dim buffer_goban(18, 18) As Integer
    Dim buffer_intensity(18, 18) As Integer
    Dim whitecaptured As Integer
    Dim blackcaptured As Integer
    Dim checkspecial As Boolean
    Dim clean_goban(18, 18) As Integer
    Dim gr As Integer
    Dim tempGroup(18, 18) As Integer
    Dim IDGR(18, 18) As Integer
    Dim intensity(18, 18) As Integer
    Dim killingeyes(2) As ArrayList
    Dim KOPoint(10) As Point
    Dim sl As Integer
    Dim GLColour As Integer
    Dim GLTotal As Integer
    Dim strongLinks(100, 2) As Integer
    Dim whiteStones As Integer
    Dim blackStones As Integer
    Dim origBoard(18, 18) As Integer
    Dim seki(900) As Point
    Dim whiteTerritory As Double
    Dim blackTerritory As Double

    Private Function Friendly(x As Integer, y As Integer, colour As Integer) As Boolean
        ' verifies the point (x,y) to be friendly (either outside the goban or inside it and empty/same colour/opposite colour cannot play there)
        If (Internal(x, y) AndAlso board(x, y) <> -colour AndAlso Not InAtari(x, y, colour)) OrElse Not Internal(x, y) Then Return True Else Return False

    End Function

    Public Sub stringAnalysis(S As Integer, display As Boolean, Optional killed As Integer = 0)
        ' determines the basic properties of a string
#If GUI Then
        If display Then Main.resetImage()
        Dim eyes, eyelikes, specialEyes As String
#End If
        Dim i, j, r As Integer
        Dim x, y As Integer
        Dim libreal, libs As Integer
        Dim p As Point

        ' the points that will count as an eye (if they are "stealing eyes") are initizialised
        killingeyes(0) = New ArrayList
        killingeyes(2) = New ArrayList
        gr = Chains(S).group
        For i = 1 To groups(gr).size
            For j = 1 To Chains(groups(gr).element(i)).size
                x = Chains(groups(gr).element(i)).p(j).X
                y = Chains(groups(gr).element(i)).p(j).Y
#If GUI Then
                ' stones belonging to to chain/(red)group(magenta) are drawn in the main program
                If display Then
                    If groups(gr).element(i) = S Then Main.plot(ministone, x, y, Color.Red) Else Main.plot(ministone, x, y, Color.Magenta)
                End If
#End If
            Next j
        Next i

#If GUI Then
        If display Then Main.debug("chain number " + Format(S) + vbCrLf)
#End If
        ' chain's liberties are computed
        Chains(S).liberty.Clear()
        Chains(S).liberties = funct(S, "LS")
        libreal = CInt(Chains(S).liberties)
        ' liberty = points of liberty for the chain; liberties = their number, that will be updated...
        For Each p In Chains(S).liberty
            ' ... because the ones the enemy cannot occupy are worth twice (if there is more than one)...
            If InAtari(p.X, p.Y, -Chains(S).colour, True) AndAlso Chains(S).liberties > 1 Then Chains(S).liberties += 1
            ' ... the ones that may be occupied by stones of the same colour without decreasing the overall number are worth 1/3 more...
            board(p.X, p.Y) = Chains(S).colour
            libs = GroupLib(p.X, p.Y)
            If libs >= libreal And libreal > 1 Then Chains(S).liberties += 1 / 3
            ' ... furthermore, if liberties increases a lot when one of them is occupied by a friendly stone, this one is worth 1/2 more
            If libs - libreal >= 2 And libreal > 1 Then Chains(S).liberties += 0.5
            board(p.X, p.Y) = 0
        Next
        ' ... first such liberty is worth 1/2 more, not just 1/3 (another 1/6 is added to the count)
        If Chains(S).liberties - libreal > 0 Then Chains(S).liberties += 1 / 6
        ' if a liberty deals with a KO, a compensation is added
        If bonus(S) Then Chains(S).liberties += 0.5
#If GUI Then
        If display Then Main.debug(Format(Chains(S).size) + " stones - " + Format(Chains(S).liberty.Count) + " liberties (are worth " + Format(Chains(S).liberties, "0.###") + ")" + vbCrLf)
        If display Then Main.debug("belongs to" + vbCrLf + vbCrLf)
        If display Then Main.debug("Group " + Format(gr) + " (" + Format(groups(Chains(S).group).stones) + " stones)")
#End If
        ' eyes are computed; "eyes" are the actual points; eyesnumber is their number; the same for eyelikes and special eyes
        Chains(S).eyesnumber = funct(S, "E1")
        Chains(S).eyesnumber += funct(S, "E2")
        ' there are three types of eye: for the third only it is checked what would happen if another chain, inner and possibly dead, would disappear
        If killed <> 0 Then
            For r = 1 To Chains(killed).size : board(Chains(killed).p(r).X, Chains(killed).p(r).Y) = 0 : Next r
        End If
        Chains(S).eyesnumber += funct(S, "E3", killed)
        If killed <> 0 Then
            For r = 1 To Chains(killed).size : board(Chains(killed).p(r).X, Chains(killed).p(r).Y) = Chains(killed).colour : Next r
        End If
        Chains(S).specialEyesNumber = funct(S, "SE")
        Chains(S).eyelikesnumber = funct(S, "EL")
        ' the territory under the control of the whole group is computed
        Chains(S).groupTerritory = territoryCompute(gr)
#If GUI Then
        If groups(Chains(S).group).eyes.Count = 1 Then eyes = " eye" Else eyes = " eyes"
        If display Then Main.debug(vbCrLf + Format(groups(Chains(S).group).eyes.Count) + eyes)
        If groups(Chains(S).group).specialEyes.Count = 1 Then specialEyes = " special eye" Else specialEyes = " special eyes"
        If display Then Main.debug(vbCrLf + Format(groups(Chains(S).group).specialEyes.Count) + specialEyes)
        If groups(Chains(S).group).eyelikes.Count = 1 Then eyelikes = " eyelike" Else eyelikes = " eyelikes"
        If display Then Main.debug(vbCrLf + Format(groups(Chains(S).group).eyelikes.Count) + eyelikes)
        If display Then Main.debug(vbCrLf + Format(Chains(S).groupTerritory) + " territory's points")
#End If

    End Sub

    Private Sub Bouzy()
        ' Bouzy's routine for counting territory
        Dim i, j As Integer
        Dim dilations_number As Integer = 9
        Dim erosions_number As Integer = 21
        whiteArea = 0
        blackArea = 0
        whiteTerritory = 0
        blackTerritory = 0
        Dim noterritory As New ArrayList
        Dim position As Point
        Dim cx, cy As Integer
        Dim countgood As Integer
        Dim countdame As Integer

        Array.Copy(board, backup_board, 361)

        For i = 0 To N1 : For j = 0 To N1 : intensity(i, j) = 0 : Next j : Next i
        ' standard routine
        For i = 0 To N1
            For j = 0 To N1
                intensity(i, j) = 0
                If board(i, j) = 1 Then intensity(i, j) = 64
                If board(i, j) = -1 Then intensity(i, j) = -64
            Next j
        Next i
        For i = 1 To dilations_number : Dilate() : Next i
        For i = 1 To erosions_number : Erode() : Next i

        Array.Copy(backup_board, board, 361)
        ' if a point of territory is close to a dame point then it's not counted (unless it's also close to at least two other points of territory)
        For i = 0 To N1
            For j = 0 To N1
                countgood = 0
                countdame = 0
                If intensity(i, j) <> 0 AndAlso board(i, j) = 0 Then
                    If Internal(i + 1, j) AndAlso board(i + 1, j) = 0 AndAlso intensity(i + 1, j) = 0 Then countdame += 1
                    If Internal(i + 1, j) AndAlso board(i + 1, j) = 0 AndAlso intensity(i + 1, j) <> 0 Then countgood += 1
                    If Internal(i - 1, j) AndAlso board(i - 1, j) = 0 AndAlso intensity(i - 1, j) = 0 Then countdame += 1
                    If Internal(i - 1, j) AndAlso board(i - 1, j) = 0 AndAlso intensity(i - 1, j) <> 0 Then countgood += 1
                    If Internal(i, j + 1) AndAlso board(i, j + 1) = 0 AndAlso intensity(i, j + 1) = 0 Then countdame += 1
                    If Internal(i, j + 1) AndAlso board(i, j + 1) = 0 AndAlso intensity(i, j + 1) <> 0 Then countgood += 1
                    If Internal(i, j - 1) AndAlso board(i, j - 1) = 0 AndAlso intensity(i, j - 1) = 0 Then countdame += 1
                    If Internal(i, j - 1) AndAlso board(i, j - 1) = 0 AndAlso intensity(i, j - 1) <> 0 Then countgood += 1
                End If
                If countdame > 0 And countgood < 2 Then
                    If Not noterritory.Contains(New Point(i, j)) Then noterritory.Add(New Point(i, j))
                End If
            Next j
        Next i

        For Each position In noterritory
            cx = position.X
            cy = position.Y
            intensity(cx, cy) = 0
        Next
        ' area and territory are counted and possibly plotted in the main program
        For i = 0 To N1
            For j = 0 To N1
                If board(i, j) = -1 Then whiteArea += 1 : whiteStones += 1
                If board(i, j) = 1 Then blackArea += 1 : blackStones += 1
                If board(i, j) = 0 AndAlso intensity(i, j) < 0 AndAlso Not noterritory.Contains(New Point(i, j)) Then
                    whiteArea += 1
                    whiteTerritory += 1
#If GUI Then
                    If shows Then Main.plot(territory, i, j, Color.White)
#End If
                End If
                If board(i, j) = 0 AndAlso intensity(i, j) > 0 AndAlso Not noterritory.Contains(New Point(i, j)) Then
                    blackArea += 1
                    blackTerritory += 1
#If GUI Then
                    If shows Then Main.plot(territory, i, j, Color.Black)
#End If
                End If
            Next j
        Next i

    End Sub

    Public Function groupsDefine() As Integer

        Dim k As Integer
        Dim index, indexg As Integer
        Dim found1, found2 As Boolean
        Dim c, d As Integer
        Dim links(900, 2) As Integer
        Dim ch1, ch2 As Integer
        Dim op As Integer
        Dim counthalf(100) As Integer
        Dim linked As Boolean = False
        Dim HC As Integer
        Dim FC As Boolean
        Dim i, j As Integer
        Dim z As Integer

        For i = 0 To N1 : For j = 0 To N1 : IDGR(i, j) = 0 : Next j : Next i
        For i = 1 To 70 : groups(i).size = 0 : Next i
        ' chains are linked into groups
        index = 0 : sl = 0
        For i = 1 To 99
            ' chains in atari, inclusing at least 2 stones, don't belong to any group
            If Chains(i).size > 0 AndAlso ((funct(i, "LS") > 1 OrElse (funct(i, "LS") = 1 AndAlso Chains(i).size = 1))) Then
                For j = i + 1 To 100
                    If Chains(j).size > 0 AndAlso Chains(i).colour = Chains(j).colour AndAlso funct(j, "LS") > 1 Then
                        linked = False
                        For k = 1 To Chains(i).size
                            c = Chains(i).p(k).X
                            d = Chains(i).p(k).Y
                            ' for each pair of chains half connection (may be prevented) and full connection (can't) points are searched
                            HC = HalfConnection(c, d, j)
                            FC = FullConnection(c, d, j, True)
                            ' a half connection point has been found (the point may belong to other connections)
                            If HC > 0 AndAlso Not FC Then counthalf(i) += HC : counthalf(j) += HC
                            If FC OrElse (HC > 0 AndAlso counthalf(i) >= 1) Then
                                ' two chains sharing a full connection point or at least 2 half connection points are linked
                                If links(index, 1) <> i OrElse links(index, 2) <> j Then
                                    index += 1
                                    links(index, 1) = i
                                    links(index, 2) = j
                                    linked = True
                                End If
                                ' full connections will be useful later
                                If FC And (strongLinks(sl, 1) <> i OrElse strongLinks(sl, 2) <> j) Then sl += 1 : strongLinks(sl, 1) = i : strongLinks(sl, 2) = j
                            End If
                        Next k
                        If Not linked Then
                            For c = 0 To N1
                                For d = 0 To N1
                                    ' two chains sharing a full connection "in between" are also linked
                                    If (FullConnection(c, d, i, False) AndAlso FullConnection(c, d, j, False)) Then
                                        If links(index, 1) <> i OrElse links(index, 2) <> j Then
                                            index += 1
                                            links(index, 1) = i
                                            links(index, 2) = j
                                            linked = True
                                        End If
                                    End If
                                    If linked Then Exit For
                                Next d
                                If linked Then Exit For
                            Next c
                        End If
                    End If
                Next j
            End If
        Next i

        ' groups are built by browsing the links
        indexg = 0

        k = 0
        Do
            k += 1
            If links(k, 1) <> 0 Then
                ch1 = links(k, 1) : ch2 = links(k, 2)
                ' a new group is created
                indexg += 1
                groups(indexg).element = New Integer(50) {}
                groups(indexg).size = 2
                groups(indexg).stones = 0
                groups(indexg).colour = Chains(ch1).colour
                groups(indexg).element(1) = ch1
                groups(indexg).element(2) = ch2
                groups(indexg).id = indexg
                groups(indexg).eyes = New ArrayList
                groups(indexg).eyelikes = New ArrayList
                groups(indexg).specialEyes = New ArrayList
                groups(indexg).liberty = New ArrayList
                Chains(ch1).group = indexg
                Chains(ch2).group = indexg
                links(k, 1) = 0 : links(k, 2) = 0

                Do
                    op = 0
                    ' now looking for other chains, already linked, to be added to the newly created group
                    For z = 1 To index
                        For i = 1 To groups(indexg).size
                            If links(z, 1) = groups(indexg).element(i) Then
                                found2 = True
                                For j = 1 To groups(indexg).size
                                    If links(z, 2) = groups(indexg).element(j) Then found2 = False
                                Next j
                                If found2 = True Then
                                    groups(indexg).element(groups(indexg).size + 1) = links(z, 2)
                                    groups(indexg).size += 1
                                    Chains(links(z, 2)).group = indexg
                                    op += 1
                                End If
                                links(z, 1) = 0 : links(z, 2) = 0
                            End If
                            If links(z, 2) = groups(indexg).element(i) Then
                                found1 = True
                                For j = 1 To groups(indexg).size
                                    If links(z, 1) = groups(indexg).element(j) Then found1 = False
                                Next j
                                If found1 = True Then
                                    groups(indexg).element(groups(indexg).size + 1) = links(z, 1)
                                    groups(indexg).size += 1
                                    Chains(links(z, 1)).group = indexg
                                    op += 1
                                End If
                                links(z, 1) = 0 : links(z, 2) = 0
                            End If
                        Next i
                    Next z
                    ' exit when all links have been scanned
                    If op = 0 Then Exit Do
                    Application.DoEvents()
                Loop
            End If
            ' exit the main cycle after every possible pair of chains has been examined
            If k >= index Then Exit Do
            Application.DoEvents()
        Loop

        ' now looking for the remaining chains, the ones that cannot be linked to other ones
        For k = 1 To 100
            If Chains(k).size > 0 Then
                found1 = True
                For i = 1 To indexg
                    For j = 1 To groups(i).size
                        If groups(i).element(j) = k Then found1 = False
                    Next j
                Next i
                ' one found; its group is created
                If found1 = True Then
                    indexg += 1
                    groups(indexg).element = New Integer(20) {}
                    groups(indexg).colour = Chains(k).colour
                    groups(indexg).element(1) = k
                    groups(indexg).id = indexg
                    groups(indexg).size = 1
                    groups(indexg).stones = 0
                    groups(indexg).eyes = New ArrayList
                    groups(indexg).eyelikes = New ArrayList
                    groups(indexg).specialEyes = New ArrayList
                    groups(indexg).liberty = New ArrayList
                    Chains(k).group = indexg
                End If
            End If
        Next k

        For i = 1 To indexg
            For j = 1 To groups(i).size
                For k = 1 To Chains(groups(i).element(j)).size
                    c = Chains(groups(i).element(j)).p(k).X
                    d = Chains(groups(i).element(j)).p(k).Y
                    IDGR(c, d) = i
                Next k
            Next j
        Next i

        Return indexg

    End Function

    Public Function ScoreCompute(ByVal NN1 As Integer, ByVal japanese As Boolean, ByVal komi As Double, ByVal hc As Integer, ByVal whitemoves As Integer, ByVal blackmoves As Integer) As Double
        ' main routine
#If GUI Then
        Dim victory As String
#End If
        Dim i As Integer
        Dim j As Integer
        Dim remov As Boolean
        Dim winner As Double
        Dim k As Integer
        Dim z As Integer
        Dim ws As Integer
        Dim bs As Integer
        Dim formerlib(18, 18) As Integer
        Dim formerstat(18, 18) As Integer
        Dim KON As Integer

        N1 = NN1
        whiteStones = 0
        blackStones = 0

        ' memorizes the original position
        Array.Copy(board, origBoard, 361)

        ' bonuses for KOs that won't be filled (not enough liberties)
        For i = 1 To 100 : bonus(i) = False : Next i
        ' looks for KOs' connection points
        KON = KOfilling()
        ' for the last cycle: removing the "stealing eyes" chains
        checkspecial = True

        Do
            ' main cycle: dead chains are removed
            For i = 1 To 100 : Chains(i) = Nothing : Next i
            For i = 1 To 70 : groups(i) = Nothing : Next i
            ' chains, groups and statuses are defined
            StringNumber = chainDefine()
            GroupNumber = groupsDefine()
            statusCompute()

            ' looking for snap-backs
            snapbackSearch()
            ' looking for sekis
            z = sekiSearch()

            ' dead chains are removed
            remov = False
            Dim stat As Integer = 520
            Do Until stat = 498
                If Not remov Then
                    For i = 1 To 100
                        If Chains(i).status = stat Then
                            chainRemove(i)
                            remov = True
                        End If
                    Next i
                    stat -= 1
                Else
                    Exit Do
                End If
            Loop
            ' last cycle: chains that look alive but include some "stealing eyes" stones are also removed
            If stat = 498 Then
                If Not remov And checkspecial = False Then Exit Do Else checkspecial = False
            End If
            Application.DoEvents()
        Loop

        ' after dead chains' removal the new position is memorized
        Array.Copy(board, clean_goban, 361)
        For i = 0 To N1
            For j = 0 To N1
                ' for each point on the goban, the status of the chain standing upon (if the point is not empty) is checked
                If ID(i, j) <> 0 Then formerstat(i, j) = Chains(ID(i, j)).status
            Next j
        Next i

        ' first territory computing
        Bouzy()

        ' dame points and forced connections are checked
        Dim s As Integer
        Dim connected As Boolean
        Dim whiteConnections As New ArrayList
        Dim blackConnections As New ArrayList
        Dim tl As Integer
        Dim status As Integer
        Dim position As Point
        Dim cx, cy As Integer
        Dim p As Point

        ' the original position is restored, and the chains are again defined
        Array.Copy(origBoard, board, 361)
        Array.Copy(intensity, backup_intensity, 361)
        StringNumber = chainDefine()

        ' for each point on the goban the number of liberties of the chain possibily standing upon is now computed
        For i = 0 To N1
            For j = 0 To N1
                formerlib(i, j) = 2
                If board(i, j) <> 0 AndAlso ID(i, j) = 0 Then formerlib(i, j) = 1
                If board(i, j) <> 0 AndAlso ID(i, j) <> 0 Then formerlib(i, j) = funct(ID(i, j), "LS")
                If board(i, j) <> 0 AndAlso formerstat(i, j) = 0 Then formerstat(i, j) = 500
            Next j
        Next i

        Dim contadame As Integer
        For i = 0 To N1
            For j = 0 To N1
                If board(i, j) = 0 AndAlso intensity(i, j) = 0 Then ' all dame points...
                    board(i, j) = 1 ' ... are filled with black stones
                    contadame += 1
                End If
            Next j
        Next i

        ' now chains and groups are again defined; liberties are again counted
        StringNumber = chainDefine()
        GroupNumber = groupsDefine()
        For k = 1 To 100
            If Chains(k).size > 0 Then Chains(k).liberties = funct(k, "LS")
        Next k

        Do ' after dame filling it's time to check if some chains are now in atari or dead
            connected = False
            For s = 1 To 100
                If Chains(s).size > 0 Then
                    tl = 0
                    For k = 1 To Chains(s).size
                        tl += formerlib(Chains(s).p(k).X, Chains(s).p(k).Y)
                    Next k
                    ' this is the chain status before dame filling (with black stones)
                    status = formerstat(Chains(s).p(1).X, Chains(s).p(1).Y)
                    ' if liberties are 0/1 a forced connection is needed
                    If Chains(s).liberties <= 1 AndAlso tl > Chains(s).size AndAlso status < 498 Then
                        ' is it possibile to capture some dead chain in order to increase liberties?
                        position = removeAtari(s, CInt(Chains(s).liberties))
                        cx = position.X
                        cy = position.Y
                        ' if it is not, the chain's remaining liberty is found
                        If cx = 99 Then
                            If Chains(s).liberty.Count > 0 Then
                                position = CType(Chains(s).liberty.Item(0), Point)
                                cx = position.X
                                cy = position.Y
                            Else
                            End If
                        End If

                        ' this point (remaining liberty or capture's point) could be a forced connection
                        If cx <> 99 And Not blackConnections.Contains(position) Then
                            blackConnections.Add(position)
                            ' thus it's filled, and again chains and groups are defined and liberties computed
                            board(cx, cy) = Chains(s).colour
                            checkCaptures(cx, cy)
                            StringNumber = chainDefine()
                            GroupNumber = groupsDefine()
                            For k = 1 To 100
                                If Chains(k).size > 0 Then Chains(k).liberties = funct(k, "LS")
                            Next k
                            connected = True
                        Else
                            ' if no point has been found, another way is tried: fill the chain's neighbour points with white stones instead of black ones
                            If Chains(s).liberties = 0 Then
                                For Each p In Chains(s).neighbourPoints
                                    If intensity(p.X, p.Y) = 0 And board(p.X, p.Y) = -Chains(s).colour Then
                                        board(p.X, p.Y) = Chains(s).colour
                                        ' as soon as the chain has got more liberties no more points are filled with white stones
                                        If GroupLib(p.X, p.Y) > 0 Then Exit For
                                    End If
                                Next
                            End If
                        End If
                    End If
                End If
                If connected Then Exit For
            Next s
            Application.DoEvents()
            ' everything is repeated until each connection has been found
        Loop Until connected = False

        Array.Copy(origBoard, board, 361)
        Array.Copy(backup_intensity, intensity, 361)
        ' the task is repeated one more, this time filling dame with white stones
        For i = 0 To N1
            For j = 0 To N1
                If board(i, j) = 0 AndAlso intensity(i, j) = 0 Then
                    board(i, j) = -1
                End If
            Next j
        Next i

        StringNumber = chainDefine()
        GroupNumber = groupsDefine()
        For k = 1 To 100
            If Chains(k).size > 0 Then Chains(k).liberties = funct(k, "LS")
        Next k

        Do
            connected = False
            For s = 1 To 100
                If Chains(s).size > 0 Then
                    tl = 0
                    For k = 1 To Chains(s).size
                        tl += formerlib(Chains(s).p(k).X, Chains(s).p(k).Y)
                    Next k
                    status = formerstat(Chains(s).p(1).X, Chains(s).p(1).Y)
                    If Chains(s).liberties <= 1 AndAlso tl > Chains(s).size AndAlso status < 498 Then ' va effettuata la connessione
                        position = removeAtari(s, CInt(Chains(s).liberties))
                        cx = position.X
                        cy = position.Y
                        If cx = 99 Then
                            If Chains(s).liberty.Count > 0 Then
                                position = CType(Chains(s).liberty.Item(0), Point)
                                cx = position.X
                                cy = position.Y
                            Else
                            End If
                        End If
                        If cx <> 99 And Not whiteConnections.Contains(position) Then
                            whiteConnections.Add(position)
                            board(cx, cy) = Chains(s).colour
                            checkCaptures(cx, cy)
                            StringNumber = chainDefine()
                            GroupNumber = groupsDefine()
                            For k = 1 To 100
                                If Chains(k).size > 0 Then Chains(k).liberties = funct(k, "LS")
                            Next k
                            connected = True
                        Else
                            If Chains(s).liberties = 0 Then
                                For Each p In Chains(s).neighbourPoints
                                    If intensity(p.X, p.Y) = 0 And board(p.X, p.Y) = -Chains(s).colour Then
                                        board(p.X, p.Y) = Chains(s).colour
                                        If GroupLib(p.X, p.Y) > 0 Then Exit For
                                    End If
                                Next
                            End If
                        End If
                    End If
                End If
                If connected Then Exit For
            Next s
            Application.DoEvents()
        Loop Until connected = False

        Dim minusblack As Integer = 0
        Dim minuswhite As Integer = 0
        ' now the forced connections found by filling dame with black stones are compared with the ones found by filling dame with white stones
        ' if a forced connection does exist in both cases, it is real
        For Each position In blackConnections
            If whiteConnections.Contains(position) Then
                cx = position.X
                cy = position.Y
                If Math.Sign(intensity(cx, cy)) > 0 Then
                    ' if the point to force connect was under black influence, black territory will be decreased
                    minusblack += 1
#If GUI Then
                    If shows Then Main.plot(connection, cx, cy, Color.Black)
#End If
                Else
                    ' same thing if the point was under white influence
                    minuswhite += 1
#If GUI Then
                    If shows Then Main.plot(connection, cx, cy, Color.White)
#End If
                End If
                checkCaptures(cx, cy)
            End If
        Next

        ' again sekis are looked for, as sometimes the forced connections would highlight some others
        z = sekiSearch()

        ' the original position is now restored
        Array.Copy(origBoard, board, 361)
        StringNumber = chainDefine()
        GroupNumber = groupsDefine()

        Dim atari As Point
        Dim connections As New ArrayList
        Dim colour(N1, N1) As Integer

        ' other forced connections: the ones required to capture a dead chain that otherwise would prevent friendly territory from being counted
        For i = 0 To N1
            For j = 0 To N1
                If intensity(i, j) = 0 AndAlso board(i, j) <> 0 Then
                    k = ID(i, j)
                    If funct(k, "LS") = 1 Then
                        ' this chain is in atari, thus its last liberty is connected
                        atari = CType(Chains(k).liberty.Item(0), Point)
                        If Not connections.Contains(atari) Then
                            connections.Add(atari)
                            colour(atari.X, atari.Y) = Chains(k).colour
                            board(atari.X, atari.Y) = Chains(k).colour
                            If GroupLib(atari.X, atari.Y) <= 1 Then
                                ' if the chain remains in atari, then the chain must be captured
                                colour(atari.X, atari.Y) = -Chains(k).colour
                                ' bonus point to compensate the stone required to capture the chain: this type of connection is quite different from the previous one!
                                If Chains(k).colour = 1 Then minusblack += 1 Else minuswhite += 1
                            End If
                            board(atari.X, atari.Y) = 0
                        End If
                    End If
                End If
            Next j
        Next i

        Array.Copy(clean_goban, board, 361)
        ' the board after the removal of dead chains is restored, and connection piints are counted
        For Each position In connections
            cx = position.X
            cy = position.Y
            board(cx, cy) = colour(cx, cy)
            If board(cx, cy) = 1 Then
                ' black connection point: black territory will decrease
                minusblack += 1
#If GUI Then
                If shows Then Main.plot(connection, cx, cy, Color.Black)
#End If
            Else
                ' white connection point: white territory will decrease
                minuswhite += 1
#If GUI Then
                If shows Then Main.plot(connection, cx, cy, Color.White)
#End If
            End If
            checkCaptures(cx, cy)
        Next
        ' now it's the moment for KOs filling
        For k = 1 To KON
            cx = Math.Abs(KOPoint(k).X)
            cy = Math.Abs(KOPoint(k).Y)
            If Math.Sign(intensity(cx, cy)) = Math.Sign(KOPoint(k).X) Then
                board(cx, cy) = Math.Sign(KOPoint(k).X)
                If board(cx, cy) = 1 Then
                    ' again, if KO is black, black territory will decrease
                    minusblack += 1
#If GUI Then
                    If shows Then Main.plot(connection, cx, cy, Color.Black)
#End If
                Else
                    ' again, if KO is white, white territory will decrease
                    minuswhite += 1
#If GUI Then
                    If shows Then Main.plot(connection, cx, cy, Color.White)
#End If
                End If
            End If
        Next k
        ' now it's moment to bring back to life stones in seki
        For k = 1 To z
            cx = Math.Abs(seki(k).X)
            cy = Math.Abs(seki(k).Y)
            If board(cx, cy) = 0 And intensity(cx, cy) <> 0 Then
                If Math.Sign(Math.Sign(seki(k).X)) > 0 Then blackStones += 1 Else whiteStones += 1
            End If
            board(cx, cy) = Math.Sign(seki(k).X)
        Next k
        ' not really necessary, unless the main program is interested in checking the ultimate status of the chains
        statusCompute()
        ' territory is computed (dead strings removed, KOs closed, seki brought back to life, connections counted)
        ws = whiteStones : bs = blackStones
        Bouzy()
        whiteStones = ws : blackStones = bs
        ' compensation for handicap, if present (territory scoring)
        blackStones -= hc
        blackcaptured = blackmoves - blackStones
        whitecaptured = whitemoves - whiteStones
        ' territory is decreased because of the forced connections...
        blackTerritory -= minusblack
        whiteTerritory -= minuswhite
        ' ... but area is increased instead
        whiteArea += minusblack
        blackArea += minuswhite
        ' compensation for handicap, if present (area scoring)
        whiteArea += hc

        If japanese Then
            winner = blackTerritory + whitecaptured - whiteTerritory - blackcaptured - komi
        Else
            winner = blackArea - whiteArea - komi
        End If
        ' too many dame points suggest a possible resignation (54 for 19x19, 24 for 13x13)
        If contadame >= (N1 + 1) * (N1 + 1) \ 6 Then winner = 1000 * Math.Sign(winner)
#If GUI Then
        victory = ""
        Select Case winner
            Case 0 : victory = "Jigo" : rs = "W + 0"
            Case > 0 : victory = "Black wins by    " + Format(winner, "##0") + " points" : rs = "B + " + Format(winner)
            Case < 0 : victory = "White wins by    " + Format(-winner, "##0") + " points" : rs = "W + " + Format(-winner)
            Case 1000 : victory = "Black wins by resignation" : rs = "B + resign"
            Case -1000 : victory = "White wins by resignation" : rs = "W + resign"
        End Select
        If shows Then
            Main.score("Black territory:      " + Format(blackTerritory, "##0"))
            Main.score(vbCrLf + "White prisoners:      " + Format(whitecaptured, "##0"))
            Main.score(vbCrLf + "--------------------  " + Format(blackTerritory + whitecaptured, "##0"))
            Main.score(vbCrLf + "White territory:      " + Format(whiteTerritory, "##0"))
            Main.score(vbCrLf + "Black prisoners:      " + Format(blackcaptured, "##0"))
            Main.score(vbCrLf + "--------------------  " + Format(whiteTerritory + blackcaptured))
            Main.score(vbCrLf + "Black area:          " + Format(blackArea, "##0"))
            Main.score(vbCrLf + "White area:          " + Format(whiteArea, "##0"))
            Main.score(vbCrLf + victory)
        End If
#End If

        Return winner

    End Function

    Private Sub statusCompute()

        Dim i, j As Integer
        Dim status As Integer
        Dim h, k, l As Integer
        Dim ch As Integer
        Dim pts As New ArrayList
        Dim neighbours As New ArrayList
        Dim neighboursNumber As Integer
        Dim pt1, pt2, pt3 As Point
        Dim cxh, cyh, cxk, cyk, cxl, cyl As Integer
        Dim d12, d23, d13 As Integer
        Dim remove As Boolean
        Dim eyesN As Integer
        Dim eye, p, liberty As Point
        ' a backup is made of the intersections' previous "intensity", as this computation will vary it ("intensity" is important when computing territory)
        Array.Copy(intensity, backup_intensity, 361)

        For i = 1 To 70
            If groups(i).size > 0 Then
                groups(i).eyes.Clear()
                groups(i).eyelikes.Clear()
                groups(i).specialEyes.Clear()
                eyesN = 0
                Do
                    groups(i).stones = 0
                    eyesN = groups(i).eyesnumber
                    For j = 1 To groups(i).size
                        ch = groups(i).element(j)
                        If ch > 0 AndAlso Chains(ch).size > 0 Then
                            ' for each chain belonging to the group eyes and liberties are counted
                            stringAnalysis(ch, False)
                            ' chain's temporary status - from 498 to 520
                            Chains(ch).status = CInt(520 - Math.Round(Chains(ch).eyesnumber / 2 + Chains(ch).liberties * 2 + Chains(ch).groupTerritory / 2))
                        End If
                    Next j
                    ' chains' stones, eyes, liberties counted before are added to group
                    For j = 1 To groups(i).size
                        ch = groups(i).element(j)
                        If ch > 0 AndAlso Chains(ch).size > 0 Then
                            For Each eye In Chains(ch).eyes
                                If Not groups(i).eyes.Contains(eye) Then groups(i).eyes.Add(eye) : groups(i).eyesnumber += 1
                            Next
                            For Each eye In Chains(ch).eyelikes
                                If Not groups(i).eyelikes.Contains(eye) Then groups(i).eyelikes.Add(eye)
                            Next
                            For Each eye In Chains(ch).specialEyes
                                If Not groups(i).specialEyes.Contains(eye) Then groups(i).specialEyes.Add(eye)
                            Next
                            For Each liberty In Chains(ch).liberty
                                If Not groups(i).liberty.Contains(liberty) Then groups(i).liberty.Add(liberty)
                            Next
                            groups(i).stones += Chains(ch).size
                        End If
                    Next j
                    groups(i).territory = Chains(ch).groupTerritory
                    ' two neighbour eyelikes/special eyes (at least one of them must be a special eye) count as one eye, so these configurations are looked for
                    pts.Clear()
                    For Each p In groups(i).specialEyes
                        pts.Add(p)
                    Next
                    For Each p In groups(i).eyelikes
                        If Not pts.Contains(p) Then pts.Add(New Point(100 + p.X, p.Y))
                    Next
                    neighboursNumber = 0
                    neighbours.Clear()
                    If pts.Count > 1 Then
                        remove = False
                        For h = 0 To pts.Count - 2
                            If CType(pts.Item(h), Point).X > 100 Then cxh = CType(pts.Item(h), Point).X - 100 Else cxh = CType(pts.Item(h), Point).X
                            cyh = CType(pts.Item(h), Point).Y
                            For k = h + 1 To pts.Count - 1
                                If CType(pts.Item(k), Point).X > 100 Then cxk = CType(pts.Item(k), Point).X - 100 Else cxk = CType(pts.Item(k), Point).X
                                cyk = CType(pts.Item(k), Point).Y
                                If ((cxh = cxk And Math.Abs(cyh - cyk) = 1) Or (cyh = cyk And Math.Abs(cxh - cxk) = 1)) And (CType(pts.Item(h), Point).X < 100 Or CType(pts.Item(k), Point).X < 100) Then
                                    ' two neighbours have been found!
                                    neighboursNumber += 1
                                    If Not neighbours.Contains(New Point(cxh, cyh)) Then neighbours.Add(New Point(cxh, cyh))
                                    If Not neighbours.Contains(New Point(cxk, cyk)) Then neighbours.Add(New Point(cxk, cyk))
                                End If
                            Next k
                        Next h
                    End If
                    Select Case neighboursNumber
                        Case 0
                        Case 1 : pt1 = CType(neighbours.Item(0), Point) : pt2 = CType(neighbours.Item(1), Point)
                            ' two neighbour points (eyelikes/special eyes)
                            If Not groups(i).eyes.Contains(pt1) Then
                                ' notice the both points behave like eyes, but the number of eyes only increases by 1
                                groups(i).eyes.Add(pt1)
                                groups(i).eyesnumber += 1
                                If Not groups(i).eyes.Contains(pt2) Then groups(i).eyes.Add(pt2)
                            End If
                            ' the eyelikes/special eyes that were merged into a true eye are removed
                            If groups(i).eyelikes.Contains(pt1) Then groups(i).eyelikes.Remove(pt1)
                            If groups(i).eyelikes.Contains(pt2) Then groups(i).eyelikes.Remove(pt2)
                            If groups(i).specialEyes.Contains(pt1) Then groups(i).specialEyes.Remove(pt1)
                            If groups(i).specialEyes.Contains(pt2) Then groups(i).specialEyes.Remove(pt2)
                        Case 2 : If neighbours.Count = 3 Then
                                ' three points, two neighbours are again worth one eye
                                pt1 = CType(neighbours.Item(0), Point) : pt2 = CType(neighbours.Item(1), Point) : pt3 = CType(neighbours.Item(2), Point)
                                If Not groups(i).eyes.Contains(pt1) Then
                                    ' the same as before; now all three points behave like eyes
                                    groups(i).eyes.Add(pt1)
                                    groups(i).eyesnumber += 1
                                    If Not groups(i).eyes.Contains(pt2) Then groups(i).eyes.Add(pt2)
                                    If Not groups(i).eyes.Contains(pt3) Then groups(i).eyes.Add(pt3)
                                End If
                                ' removal of the eyelikes/special eyes
                                If groups(i).eyelikes.Contains(pt1) Then groups(i).eyelikes.Remove(pt1)
                                If groups(i).eyelikes.Contains(pt2) Then groups(i).eyelikes.Remove(pt2)
                                If groups(i).eyelikes.Contains(pt3) Then groups(i).eyelikes.Remove(pt3)
                                If groups(i).specialEyes.Contains(pt1) Then groups(i).specialEyes.Remove(pt1)
                                If groups(i).specialEyes.Contains(pt2) Then groups(i).specialEyes.Remove(pt2)
                                If groups(i).specialEyes.Contains(pt3) Then groups(i).specialEyes.Remove(pt3)
                            Else
                                ' four or more points, two neighbours, lareg territory are worth two eyes
                                If groups(i).territory > 2 Then groups(i).eyesnumber = 2
                            End If
                            ' more than two neighbours, large territory are also worth two eyes
                        Case > 2 : If groups(i).territory > 3 Then groups(i).eyesnumber = 2
                    End Select
                    ' three neighbours, all eyelikes (in the previous cases at least one special eye was needed) may be merged into a true eye
                    If groups(i).eyelikes.Count > 2 Then
                        Do
                            remove = False
                            For h = 0 To groups(i).eyelikes.Count - 3
                                cxh = CType(groups(i).eyelikes.Item(h), Point).X
                                cyh = CType(groups(i).eyelikes.Item(h), Point).Y
                                For k = h + 1 To groups(i).eyelikes.Count - 2
                                    cxk = CType(groups(i).eyelikes.Item(k), Point).X
                                    cyk = CType(groups(i).eyelikes.Item(k), Point).Y
                                    For l = k + 1 To groups(i).eyelikes.Count - 1
                                        cxl = CType(groups(i).eyelikes.Item(l), Point).X
                                        cyl = CType(groups(i).eyelikes.Item(l), Point).Y
                                        d12 = Math.Abs(cxh - cxk) + Math.Abs(cyh - cyk)
                                        d23 = Math.Abs(cxh - cxl) + Math.Abs(cyh - cyl)
                                        d13 = Math.Abs(cxk - cxl) + Math.Abs(cyk - cyl)
                                        If (d12 = 1 And d23 = 1) Or (d12 = 1 And d13 = 1) Or (d23 = 1 And d12 = 1) Then
                                            ' that's the case: one eye is added, three eyelikes are removed, just like before
                                            If Not groups(i).eyes.Contains(groups(i).eyelikes.Item(h)) Then
                                                groups(i).eyes.Add(groups(i).eyelikes.Item(h))
                                                If Not groups(i).eyes.Contains(groups(i).eyelikes.Item(k)) Then groups(i).eyes.Add(groups(i).eyelikes.Item(k))
                                                If Not groups(i).eyes.Contains(groups(i).eyelikes.Item(l)) Then groups(i).eyes.Add(groups(i).eyelikes.Item(l))
                                                groups(i).eyelikes.Remove(groups(i).eyelikes.Item(l))
                                                groups(i).eyelikes.Remove(groups(i).eyelikes.Item(k))
                                                groups(i).eyelikes.Remove(groups(i).eyelikes.Item(h))
                                                groups(i).eyesnumber += 1
                                                remove = True
                                            End If
                                        End If
                                        If remove Then Exit For
                                    Next l
                                    If remove Then Exit For
                                Next k
                                If remove Then Exit For
                            Next h
                            Application.DoEvents()
                        Loop While remove = True
                    End If
                    ' each time an eye is added, the chains' properties are again counted (the presence of new eyes may possibily change them)
                    Application.DoEvents()
                Loop While eyesN <> groups(i).eyesnumber
            End If
        Next i

        For i = 1 To 100
            If Chains(i).size > 0 Then
                ' chains linked by means of a full connection get the status of the best one among them (the lowest status)
                For l = 1 To sl
                    If strongLinks(l, 1) = i Then
                        status = Chains(strongLinks(l, 2)).status
                        If status < Chains(i).status Then Chains(i).status = status
                    End If
                    If strongLinks(l, 2) = i Then
                        status = Chains(strongLinks(l, 1)).status
                        If status < Chains(i).status Then Chains(i).status = status
                    End If
                Next l
                ' groups with two eyes are alive
                If groups(Chains(i).group).eyesnumber >= 2 Then Chains(i).status = 100
                ' groups with one eye only are alive if...
                If groups(Chains(i).group).eyesnumber = 1 Then
                    Select Case Chains(i).groupTerritory
                        ' ... either control three points of territory, and each one is eye/special eye
                        Case = 3 : If groups(Chains(i).group).eyesnumber + groups(Chains(i).group).specialEyes.Count = 3 Then Chains(i).status = 100
                        ' ... or control more than three points of territory
                        Case > 3 : Chains(i).status = 100
                    End Select
                End If
                ' groups with no eyes are alive if control at least 6 points of territory
                If groups(Chains(i).group).eyesnumber = 0 And Chains(i).groupTerritory >= 6 Then Chains(i).status = 100
                ' "stealing eyes" chains ("rabbit ears" and so on) are alive despite looking dead
                If checkspecial AndAlso Killing(i) Then Chains(i).status = 100
            End If
        Next i

        Array.Copy(backup_intensity, intensity, 361)

    End Sub

    Public Function chainDefine() As Integer

        Dim StringNumber As Integer
        Dim i, j, s As Integer
        Dim size As Integer
        Dim oldstring, newstring As Integer
        Dim ch As Integer
        Dim cx, cy As Integer

        For i = 0 To N1 : For j = 0 To N1 : ID(i, j) = 0 : Next j : Next i
        For i = 1 To 100 : Chains(i).size = 0 : Next i
        StringNumber = 0
        ' identifies the goban's chains
        For i = 0 To N1
            For j = 0 To N1
                If board(i, j) <> 0 Then
                    If i > 0 AndAlso ID(i - 1, j) <> 0 AndAlso Chains(ID(i - 1, j)).colour = board(i, j) Then
                        ' on the left there is a stone, and it belongs to a chain already identified
                        ch = ID(i - 1, j)
                        ID(i, j) = ch
                        Chains(ch).size += 1
                        size = Chains(ch).size
                        Chains(ch).p(size) = New Point(i, j)
                    End If
                    If j > 0 AndAlso ID(i, j - 1) <> 0 AndAlso Chains(ID(i, j - 1)).colour = board(i, j) Then
                        ' above there is a stone, and it belongs to a chain alreadu identified
                        If ID(i, j) = 0 Then
                            ch = ID(i, j - 1)
                            ID(i, j) = ch
                            Chains(ch).size += 1
                            size = Chains(ch).size
                            Chains(ch).p(size) = New Point(i, j)
                        Else
                            ' two chains are found to be the same one, so are merged
                            oldstring = ID(i, j - 1)
                            newstring = ID(i, j)
                            If oldstring <> newstring Then
                                For s = 1 To Chains(oldstring).size
                                    ID(Chains(oldstring).p(s).X, Chains(oldstring).p(s).Y) = newstring
                                    Chains(newstring).size += 1
                                    size = Chains(newstring).size
                                    Chains(newstring).p(size) = Chains(oldstring).p(s)
                                Next s
                                Chains(oldstring).size = 0
                            End If
                        End If
                    End If
                    ' a new chain is created
                    If ID(i, j) = 0 Then
                        StringNumber = StringNumber + 1
                        Chains(StringNumber).id = StringNumber
                        Chains(StringNumber).colour = board(i, j)
                        Chains(StringNumber).size = 1
                        Chains(StringNumber).p = New Point(150) {}
                        Chains(StringNumber).p(1) = New Point(i, j)
                        Chains(StringNumber).emptyNeighbourPoints = New ArrayList
                        Chains(StringNumber).neighbourPoints = New ArrayList
                        Chains(StringNumber).eyes = New ArrayList
                        Chains(StringNumber).eyelikes = New ArrayList
                        Chains(StringNumber).specialEyes = New ArrayList
                        Chains(StringNumber).liberty = New ArrayList
                        ID(i, j) = StringNumber
                    End If
                End If
            Next j
        Next i

        StringNumber = 0
        For i = 1 To 100
            If Chains(i).size > 0 Then
                StringNumber = StringNumber + 1
                ' the chain's neighbour points are counted; empty points are counted again
                For j = 1 To Chains(i).size
                    cx = Chains(i).p(j).X
                    cy = Chains(i).p(j).Y
                    If Internal(cx - 1, cy - 1) AndAlso Not Chains(i).neighbourPoints.Contains(New Point(cx - 1, cy - 1)) Then
                        Chains(i).neighbourPoints.Add(New Point(cx - 1, cy - 1))
                        If board(cx - 1, cy - 1) = 0 AndAlso Not Chains(i).emptyNeighbourPoints.Contains(New Point(cx - 1, cy - 1)) Then Chains(i).emptyNeighbourPoints.Add(New Point(cx - 1, cy - 1))
                    End If
                    If Internal(cx, cy - 1) AndAlso Not Chains(i).neighbourPoints.Contains(New Point(cx, cy - 1)) Then
                        Chains(i).neighbourPoints.Add(New Point(cx, cy - 1))
                        If board(cx, cy - 1) = 0 AndAlso Not Chains(i).emptyNeighbourPoints.Contains(New Point(cx, cy - 1)) Then Chains(i).emptyNeighbourPoints.Add(New Point(cx, cy - 1))
                    End If
                    If Internal(cx + 1, cy - 1) AndAlso Not Chains(i).neighbourPoints.Contains(New Point(cx + 1, cy - 1)) Then
                        Chains(i).neighbourPoints.Add(New Point(cx + 1, cy - 1))
                        If board(cx + 1, cy - 1) = 0 AndAlso Not Chains(i).emptyNeighbourPoints.Contains(New Point(cx + 1, cy - 1)) Then Chains(i).emptyNeighbourPoints.Add(New Point(cx + 1, cy - 1))
                    End If
                    If Internal(cx - 1, cy) AndAlso Not Chains(i).neighbourPoints.Contains(New Point(cx - 1, cy)) Then
                        Chains(i).neighbourPoints.Add(New Point(cx - 1, cy))
                        If board(cx - 1, cy) = 0 AndAlso Not Chains(i).emptyNeighbourPoints.Contains(New Point(cx - 1, cy)) Then Chains(i).emptyNeighbourPoints.Add(New Point(cx - 1, cy))
                    End If
                    If Internal(cx + 1, cy) AndAlso Not Chains(i).neighbourPoints.Contains(New Point(cx + 1, cy)) Then
                        Chains(i).neighbourPoints.Add(New Point(cx + 1, cy))
                        If board(cx + 1, cy) = 0 AndAlso Not Chains(i).emptyNeighbourPoints.Contains(New Point(cx + 1, cy)) Then Chains(i).emptyNeighbourPoints.Add(New Point(cx + 1, cy))
                    End If
                    If Internal(cx - 1, cy + 1) AndAlso Not Chains(i).neighbourPoints.Contains(New Point(cx - 1, cy + 1)) Then
                        Chains(i).neighbourPoints.Add(New Point(cx - 1, cy + 1))
                        If board(cx - 1, cy + 1) = 0 AndAlso Not Chains(i).emptyNeighbourPoints.Contains(New Point(cx - 1, cy + 1)) Then Chains(i).emptyNeighbourPoints.Add(New Point(cx - 1, cy + 1))
                    End If
                    If Internal(cx, cy + 1) AndAlso Not Chains(i).neighbourPoints.Contains(New Point(cx, cy + 1)) Then
                        Chains(i).neighbourPoints.Add(New Point(cx, cy + 1))
                        If board(cx, cy + 1) = 0 AndAlso Not Chains(i).emptyNeighbourPoints.Contains(New Point(cx, cy + 1)) Then Chains(i).emptyNeighbourPoints.Add(New Point(cx, cy + 1))
                    End If
                    If Internal(cx + 1, cy + 1) AndAlso Not Chains(i).neighbourPoints.Contains(New Point(cx + 1, cy + 1)) Then
                        Chains(i).neighbourPoints.Add(New Point(cx + 1, cy + 1))
                        If board(cx + 1, cy + 1) = 0 AndAlso Not Chains(i).emptyNeighbourPoints.Contains(New Point(cx + 1, cy + 1)) Then Chains(i).emptyNeighbourPoints.Add(New Point(cx + 1, cy + 1))
                    End If
                Next j
            End If
        Next i

        Return StringNumber

    End Function

    Public Function territoryCompute(gro As Integer) As Integer

        ' the territory under a group's control is counted
        Dim i, j As Integer

        Dim bg(18, 18) As Integer
        Dim int(18, 18) As Integer
        Dim noterritory As New ArrayList
        Dim good As Boolean
        ReDim controlled(18, 18)

        For i = 0 To N1 : For j = 0 To N1 : controlled(i, j) = False : Next j : Next i

        ' to perform this calculation, friendly stones not belonging to the group must be ignored; a backup of the current situation is needed
        Array.Copy(board, bg, 361)
        Array.Copy(intensity, int, 361)

        For i = 0 To N1
            For j = 0 To N1
                If board(i, j) <> 0 AndAlso Chains(ID(i, j)).group <> gro AndAlso groups(Chains(ID(i, j)).group).colour = groups(gro).colour Then board(i, j) = 0
            Next j
        Next i
        ' Bouzy routine
        Dim dilations_number As Integer = 9
        Dim erosions_number As Integer = 21
        For i = 0 To N1
            For j = 0 To N1
                intensity(i, j) = 0
                If board(i, j) = 1 Then intensity(i, j) = 64
                If board(i, j) = -1 Then intensity(i, j) = -64
            Next j
        Next i

        For i = 1 To dilations_number : Dilate() : Next i
        For i = 1 To erosions_number : Erode() : Next i

        Array.Copy(bg, board, 361)
        ' points that seem under the group's control but are close to dame points are marked...
        For i = 0 To N1
            For j = 0 To N1
                good = False
                If Math.Sign(intensity(i, j)) = groups(gro).colour And board(i, j) = 0 Then
                    If Internal(i + 1, j) AndAlso board(i + 1, j) = 0 AndAlso intensity(i + 1, j) = 0 Then
                        If Not noterritory.Contains(New Point(i, j)) Then noterritory.Add(New Point(i, j))
                    End If
                    If Internal(i - 1, j) AndAlso board(i - 1, j) = 0 AndAlso intensity(i - 1, j) = 0 Then
                        If Not noterritory.Contains(New Point(i, j)) Then noterritory.Add(New Point(i, j))
                    End If
                    If Internal(i, j + 1) AndAlso board(i, j + 1) = 0 AndAlso intensity(i, j + 1) = 0 Then
                        If Not noterritory.Contains(New Point(i, j)) Then noterritory.Add(New Point(i, j))
                    End If
                    If Internal(i, j - 1) AndAlso board(i, j - 1) = 0 AndAlso intensity(i, j - 1) = 0 Then
                        If Not noterritory.Contains(New Point(i, j)) Then noterritory.Add(New Point(i, j))
                    End If
                    controlled(i, j) = True
                    If noterritory.Contains(New Point(i, j)) And good Then noterritory.Remove(New Point(i, j))
                End If
            Next j
        Next i

        Array.Copy(int, intensity, 361)
        ' ... in order not to be counted as territory
        Dim totale As Integer = 0
        For i = 0 To N1
            For j = 0 To N1
                If controlled(i, j) = True AndAlso Not noterritory.Contains(New Point(i, j)) Then totale += 1
            Next j
        Next i

        Return totale

    End Function

    Private Sub Erode()

        ' Bouzy routine: erosion sub-routine
        Dim i, j, r As Integer
        Dim nature As Integer
        Array.Copy(board, buffer_goban, 361)
        Array.Copy(intensity, buffer_intensity, 361)
        For i = 0 To N1
            For j = 0 To N1
                nature = buffer_goban(i, j)
                If nature <> 0 Then
                    For r = 1 To 4
                        Dim iv, jv As Integer
                        iv = veryClose(r, i, j).X
                        jv = veryClose(r, i, j).Y
                        If Internal(iv, jv) AndAlso Not buffer_goban(iv, jv) = nature Then intensity(i, j) = intensity(i, j) - Math.Sign(nature)
                        If Internal(iv, jv) AndAlso intensity(i, j) = 0 Then board(i, j) = 0 : Exit For
                    Next r
                End If
            Next j
        Next i

    End Sub

    Private Function sekiSearch() As Integer

        Dim i, j, k As Integer
        Dim liberty As Point
        Dim same As Boolean
        Dim gro, ch As Integer
        Dim cx, cy As Integer
        Dim tti, ttj As Integer
        Dim z As Integer
        Dim p As Point
        Dim falseEyes As New ArrayList

        For z = 1 To 100 : seki(z) = New Point(99, 99) : Next z

        z = 0
        ' falseEyes must be located: they count neither as liberties nor as territory (at least in seki)
        For gro = 1 To 70
            If groups(gro).size > 0 Then
                falseEyes.Clear()
                For i = 0 To groups(gro).liberty.Count - 1
                    If falseEye(CType(groups(gro).liberty(i), Point), gro) Then falseEyes.Add(groups(gro).liberty(i))
                Next i
                For i = 0 To falseEyes.Count - 1
                    groups(gro).liberty.Remove(falseEyes(i))
                    groups(gro).territory -= 1
                Next i
            End If
        Next gro

        For i = 1 To 69
            ' potential sekis:
            ' at least two stones and two liberties in each groups, no territory (not counting eyes), groups are opposite colour and look dead
            For j = i + 1 To 70
                If groups(i).size > 0 Then tti = groups(i).territory - groups(i).eyes.Count
                If groups(j).size > 0 Then ttj = groups(j).territory - groups(j).eyes.Count
                If groups(i).size > 0 AndAlso groups(i).stones > 2 AndAlso groups(j).size > 0 AndAlso groups(j).stones > 2 AndAlso Chains(groups(j).element(1)).colour <> Chains(groups(i).element(1)).colour AndAlso groups(i).liberty.Count >= 2 AndAlso groups(j).liberty.Count >= 2 AndAlso tti = 0 AndAlso ttj = 0 AndAlso Chains(groups(i).element(1)).status > 100 AndAlso Chains(groups(j).element(1)).status > 100 Then
                    Dim small, large As Integer
                    ' the group with less liberties is the "small" one (usually the inner one)
                    If groups(i).liberty.Count < groups(j).liberty.Count Then
                        small = i
                        large = j
                    Else
                        large = i
                        small = j
                    End If
                    same = False
                    ' two kinds of seki:
                    ' - the "small" group contains 3 stones or 4 squared (killing shape)
                    ' - number of liberties is the same for both groups (but liberties are <= 5)
                    If killingShape(groups(small).element(1)) OrElse (groups(small).liberty.Count <= 5 AndAlso groups(small).liberty.Count = groups(large).liberty.Count) Then same = True
                    ' first: if the groups contain an eye it does not count as liberty
                    For Each p In groups(i).eyes : If groups(i).liberty.Contains(p) Then groups(i).liberty.Remove(p)
                    Next
                    For Each p In groups(j).eyes : If groups(j).liberty.Contains(p) Then groups(j).liberty.Remove(p)
                    Next
                    ' second: let's check if after the removal of the eye the liberties are still the same number (second kind of seki only)
                    If Not killingShape(groups(small).element(1)) AndAlso groups(small).liberty.Count <> groups(large).liberty.Count Then same = False
                    ' third and last: for both kinds of seki the small group's liberties must be included in the large group's
                    For Each liberty In groups(small).liberty
                        If Not groups(large).liberty.Contains(liberty) Then same = False
                    Next
                    ' they are, so this is a seki...
                    If same Then
                        ' ... and all the chains belonging to both groups are brought back to life (status = 100)
                        For gro = 1 To groups(i).size
                            ch = groups(i).element(gro)
                            Chains(ch).status = 100
                            For k = 1 To Chains(ch).size
                                cx = Chains(ch).p(k).X
                                cy = Chains(ch).p(k).Y
                                z += 1
                                If Chains(ch).colour = 1 Then seki(z) = New Point(cx, cy) Else seki(z) = New Point(-cx, -cy)
#If GUI Then
                                If shows Then
                                    If Chains(ch).colour = 1 Then
                                        Main.plot(stone, cx, cy, Color.DimGray)
                                    Else
                                        Main.plot(stone, cx, cy, Color.LightGray)
                                    End If
                                End If
#End If
                            Next k
                        Next gro
                        For gro = 1 To groups(j).size
                            ch = groups(j).element(gro)
                            Chains(ch).status = 100
                            For k = 1 To Chains(ch).size
                                cx = Chains(ch).p(k).X
                                cy = Chains(ch).p(k).Y
                                z += 1
                                If Chains(ch).colour = 1 Then seki(z) = New Point(cx, cy) Else seki(z) = New Point(-cx, -cy)
#If GUI Then
                                If Chains(ch).colour = 1 Then
                                    Main.plot(stone, cx, cy, Color.DimGray)
                                Else
                                    Main.plot(stone, cx, cy, Color.LightGray)
                                End If
#End If
                            Next k
                        Next gro
                    End If
                End If
            Next j
        Next i
        Return z

    End Function

    Private Sub snapbackSearch()

        Dim backupsnap(18, 18) As Integer
        Dim i, j As Integer
        Dim x1, y1 As Integer
        Dim newlib As Integer

        For i = 1 To 99
            For j = i + 1 To 100
                ' snap-back may occur when two chains have only one common liberty and look dead
                If Chains(i).size > 0 AndAlso Chains(j).size > 0 AndAlso Chains(i).colour = -Chains(j).colour AndAlso funct(i, "LS") = 1 AndAlso funct(j, "LS") = 1 AndAlso Chains(i).status > 100 AndAlso Chains(j).status > 100 Then
                    If CType(Chains(i).liberty.Item(0), Point) = CType(Chains(j).liberty.Item(0), Point) Then
                        x1 = CType(Chains(i).liberty.Item(0), Point).X
                        y1 = CType(Chains(i).liberty.Item(0), Point).Y
                        Array.Copy(board, backupsnap, 361)
                        ' let's occupy this liberty with a stone of the first colour
                        board(x1, y1) = Chains(i).colour
                        checkCaptures(x1, y1)
                        ID(x1, y1) = i
                        newlib = funct(i, "LS")
                        Array.Copy(backupsnap, board, 361)
                        ID(x1, y1) = 0
                        ' if the chain whose colour we tried still has got one liberty, it dies; the other one lives
                        If newlib = 1 Then
                            Chains(i).status = 520 : Chains(j).status = 100
                        Else
                            board(x1, y1) = Chains(j).colour
                            checkCaptures(x1, y1)
                            ID(x1, y1) = j
                            ' if this is not case, we try the opposite colour
                            newlib = funct(j, "LS")
                            Array.Copy(backupsnap, board, 361)
                            ID(x1, y1) = 0
                            If newlib = 1 Then Chains(j).status = 520 : Chains(i).status = 100
                        End If
                    End If
                End If
            Next j
        Next i

    End Sub

    Public Sub checkCaptures(ByVal x As Integer, ByVal y As Integer)

        Dim xx As Integer, yy As Integer, xxx As Integer, yyy As Integer
        ' delete from the virtual board a stone that has been captured by another one plaied in (x,y)
        For xx = x - 1 To x + 1
            For yy = y - 1 To y + 1
                If Math.Abs(xx - x) + Math.Abs(yy - y) = 1 AndAlso xx >= 0 AndAlso xx <= N1 AndAlso yy >= 0 AndAlso yy <= N1 Then
                    If board(xx, yy) = -board(x, y) Then
                        If GroupLib(xx, yy) = 0 Then
                            For xxx = 0 To N1
                                For yyy = 0 To N1
                                    If tempGroup(xxx, yyy) = 1 Then board(xxx, yyy) = 0
                                Next yyy
                            Next xxx
                        End If
                    End If
                End If
            Next yy
        Next xx

    End Sub

    Private Sub Dilate()

        ' Bouzy routine: dilation sub-routine
        Dim i, j, r As Integer
        Dim nature As Integer
        Dim out As Boolean
        Array.Copy(board, buffer_goban, 361)
        Array.Copy(intensity, buffer_intensity, 361)
        For i = 0 To N1
            For j = 0 To N1
                out = False
                nature = buffer_goban(i, j)
                For r = 1 To 4
                    Dim iv, jv As Integer
                    iv = veryClose(r, i, j).X
                    jv = veryClose(r, i, j).Y
                    If Internal(iv, jv) AndAlso buffer_goban(iv, jv) <> 0 AndAlso buffer_goban(iv, jv) <> nature Then
                        If nature <> 0 Then out = True Else nature = buffer_goban(iv, jv)
                    End If
                Next r
                If out = False And nature <> 0 Then
                    board(i, j) = nature
                    For r = 1 To 4
                        Dim iv, jv As Integer
                        iv = veryClose(r, i, j).X
                        jv = veryClose(r, i, j).Y
                        If Internal(iv, jv) AndAlso buffer_goban(iv, jv) = nature Then intensity(i, j) = intensity(i, j) + Math.Sign(nature)
                    Next r
                End If
            Next j
        Next i

    End Sub

    Private Function killingShape(ch As Integer) As Boolean

        Dim good As Boolean
        Dim meanx, meany As Double
        Dim z As Integer
        Dim temp(18, 18) As Integer
        Dim tempf(18, 18) As Integer
        Dim liberty As Point

        ' chains that have "killing shape" are checked, because they could become part of a seki
        good = False

        ' three stones are a "killing shape"
        If Chains(ch).size = 3 AndAlso groups(Chains(ch).group).size = 1 Then good = True
        ' four stone in a square are a "killing shape" too
        If Chains(ch).size = 4 AndAlso groups(Chains(ch).group).size = 1 Then
            good = True
            meanx = 0 : meany = 0
            For z = 1 To 4
                meanx += Chains(ch).p(z).X
                meany += Chains(ch).p(z).Y
            Next z
            meanx = meanx / 4
            meany = meany / 4
            For z = 1 To 4
                If Math.Abs(meanx - Chains(ch).p(z).X) <> 0.5 Then good = False
                If Math.Abs(meany - Chains(ch).p(z).Y) <> 0.5 Then good = False
            Next
        End If
        If good Then
            ' "killing shape" by itself it's not enough: the chain's liberties must be "isolated", meaning that no others appear if they all are filled
            Array.Copy(ID, temp, 361)
            Array.Copy(board, tempf, 361)
            For Each liberty In Chains(ch).liberty : ID(liberty.X, liberty.Y) = ch : board(liberty.X, liberty.Y) = Chains(ch).colour
            Next
            Dim nl As Integer = funct(ch, "LS")
            ' new liberties have appeared, so the "killing shape" is not enough and the chain cannot belong to a seki
            If nl > 0 Then good = False
            Array.Copy(temp, ID, 361)
            Array.Copy(tempf, board, 361)
            nl = funct(ch, "LS")
        End If

        Return good

    End Function

    Private Function FullConnection(x As Integer, y As Integer, ch As Integer, checklink As Boolean) As Boolean

        ' is point (x,y) a possible extension for chain ch?
        Dim col As Integer = Chains(ch).colour

        ' point not empty = no extension, unless checklink = true; were that the case, the point may work as a link if the stone's colour is like the chain's
        If board(x, y) <> 0 AndAlso checklink = False Then
            Return False
        Else
            ' diagonal jump: both side points must be empty/one of them is occupied by a friendly stone/cannot be occupied by an enemy stone
            If Internal(x + 1, y + 1) AndAlso ID(x + 1, y + 1) = ch Then
                If (empty(x + 1, y) AndAlso empty(x, y + 1)) OrElse board(x + 1, y) = col OrElse board(x, y + 1) = col OrElse InAtari(x + 1, y, -col) OrElse InAtari(x, y + 1, -col) Then Return True
            End If
            If Internal(x - 1, y + 1) AndAlso ID(x - 1, y + 1) = ch Then
                If (empty(x - 1, y) AndAlso empty(x, y + 1)) OrElse board(x - 1, y) = col OrElse board(x, y + 1) = col OrElse InAtari(x - 1, y, -col) OrElse InAtari(x, y + 1, -col) Then Return True
            End If
            If Internal(x + 1, y - 1) AndAlso ID(x + 1, y - 1) = ch Then
                If (empty(x + 1, y) AndAlso empty(x, y - 1)) OrElse board(x + 1, y) = col OrElse board(x, y - 1) = col OrElse InAtari(x + 1, y, -col) OrElse InAtari(x, y - 1, -col) Then Return True
            End If
            If Internal(x - 1, y - 1) AndAlso ID(x - 1, y - 1) = ch Then
                If (empty(x - 1, y) AndAlso empty(x, y - 1)) OrElse board(x - 1, y) = col OrElse board(x, y - 1) = col OrElse InAtari(x - 1, y, -col) OrElse InAtari(x, y - 1, -col) Then Return True
            End If
            ' one point jump: the middle point must be empty, the two side points empty or friendly
            If Internal(x + 2, y) AndAlso ID(x + 2, y) = ch Then
                If (Friendly(x, y - 1, col) OrElse Friendly(x, y + 1, col)) AndAlso empty(x + 1, y - 1) AndAlso empty(x + 1, y) AndAlso empty(x + 1, y + 1) AndAlso (Friendly(x + 2, y - 1, col) OrElse Friendly(x + 2, y + 1, col)) Then Return True
            End If
            If Internal(x - 2, y) AndAlso ID(x - 2, y) = ch Then
                If (Friendly(x, y - 1, col) OrElse Friendly(x, y + 1, col)) AndAlso empty(x - 1, y - 1) AndAlso empty(x - 1, y) AndAlso empty(x - 1, y + 1) AndAlso (Friendly(x - 2, y - 1, col) OrElse Friendly(x - 2, y + 1, col)) Then Return True
            End If
            If Internal(x, y + 2) AndAlso ID(x, y + 2) = ch Then
                If (Friendly(x - 1, y, col) OrElse Friendly(x + 1, y, col)) AndAlso empty(x - 1, y + 1) AndAlso empty(x, y + 1) AndAlso empty(x + 1, y + 1) AndAlso (Friendly(x - 1, y + 2, col) OrElse Friendly(x + 1, y + 2, col)) Then Return True
            End If
            If Internal(x, y - 2) AndAlso ID(x, y - 2) = ch Then
                If (Friendly(x - 1, y, col) OrElse Friendly(x + 1, y, col)) AndAlso empty(x - 1, y - 1) AndAlso empty(x, y - 1) AndAlso empty(x + 1, y - 1) AndAlso (Friendly(x - 1, y - 2, col) OrElse Friendly(x + 1, y - 2, col)) Then Return True
            End If
            ' knight's jump: the two middle points must be empty, the two side point empty or friendly
            If Internal(x - 1, y + 2) AndAlso ID(x - 1, y + 2) = ch Then
                If Friendly(x - 1, y, col) AndAlso empty(x - 1, y + 1) AndAlso empty(x, y + 1) AndAlso Friendly(x, y + 2, col) Then Return True
            End If
            If Internal(x + 1, y + 2) AndAlso ID(x + 1, y + 2) = ch Then
                If Friendly(x + 1, y, col) AndAlso empty(x, y + 1) AndAlso empty(x + 1, y + 1) AndAlso Friendly(x, y + 2, col) Then Return True
            End If
            If Internal(x - 1, y - 2) AndAlso ID(x - 1, y - 2) = ch Then
                If Friendly(x - 1, y, col) AndAlso empty(x - 1, y - 1) AndAlso empty(x, y - 1) AndAlso Friendly(x, y - 2, col) Then Return True
            End If
            If Internal(x + 1, y - 2) AndAlso ID(x + 1, y - 2) = ch Then
                If Friendly(x + 1, y, col) AndAlso empty(x, y - 1) AndAlso empty(x + 1, y - 1) AndAlso Friendly(x, y - 2, col) Then Return True
            End If
            If Internal(x + 2, y - 1) AndAlso ID(x + 2, y - 1) = ch Then
                If Friendly(x, y - 1, col) AndAlso empty(x + 1, y - 1) AndAlso empty(x + 1, y) AndAlso Friendly(x + 2, y, col) Then Return True
            End If
            If Internal(x + 2, y + 1) AndAlso ID(x + 2, y + 1) = ch Then
                If Friendly(x, y + 1, col) AndAlso empty(x + 1, y) AndAlso empty(x + 1, y + 1) AndAlso Friendly(x + 2, y, col) Then Return True
            End If
            If Internal(x - 2, y - 1) AndAlso ID(x - 2, y - 1) = ch Then
                If Friendly(x, y - 1, col) AndAlso empty(x - 1, y - 1) AndAlso empty(x - 1, y) AndAlso Friendly(x - 2, y, col) Then Return True
            End If
            If Internal(x - 2, y + 1) AndAlso ID(x - 2, y + 1) = ch Then
                If Friendly(x, y + 1, col) AndAlso empty(x - 1, y) AndAlso empty(x - 1, y + 1) AndAlso Friendly(x - 2, y, col) Then Return True
            End If
            ' two points jump: the middle points must be empty, the neighbours empty or friendly
            If Internal(x, y + 3) AndAlso ID(x, y + 3) = ch Then
                If (Friendly(x - 1, y, col) OrElse InAtari(x, y + 1, -col)) AndAlso (Friendly(x + 1, y, col) OrElse InAtari(x, y + 1, -col)) AndAlso Friendly(x - 1, y + 1, col) AndAlso empty(x, y + 1) AndAlso Friendly(x + 1, y + 1, col) AndAlso Friendly(x - 1, y + 2, col) AndAlso empty(x, y + 2) AndAlso Friendly(x + 1, y + 2, col) AndAlso (Friendly(x - 1, y + 3, col) OrElse InAtari(x, y + 2, -col)) AndAlso (Friendly(x + 1, y + 3, col) OrElse InAtari(x, y + 2, -col)) Then Return True
            End If
            If Internal(x, y - 3) AndAlso ID(x, y - 3) = ch Then
                If (Friendly(x - 1, y, col) OrElse InAtari(x, y - 1, -col)) AndAlso (Friendly(x + 1, y, col) OrElse InAtari(x, y - 1, -col)) AndAlso Friendly(x - 1, y - 1, col) AndAlso empty(x, y - 1) AndAlso Friendly(x + 1, y - 1, col) AndAlso Friendly(x - 1, y - 2, col) AndAlso empty(x, y - 2) AndAlso Friendly(x + 1, y - 2, col) AndAlso (Friendly(x - 1, y - 3, col) OrElse InAtari(x, y - 2, -col)) AndAlso (Friendly(x + 1, y - 3, col) OrElse InAtari(x, y - 2, -col)) Then Return True
            End If
            If Internal(x - 3, y) AndAlso ID(x - 3, y) = ch Then
                If (Friendly(x, y - 1, col) OrElse InAtari(x - 1, y, -col)) AndAlso (Friendly(x, y + 1, col) OrElse InAtari(x - 1, y, -col)) AndAlso Friendly(x - 1, y - 1, col) AndAlso empty(x - 1, y) AndAlso Friendly(x - 1, y + 1, col) AndAlso Friendly(x - 2, y + 1, col) AndAlso empty(x - 2, y) AndAlso Friendly(x - 2, y - 1, col) AndAlso (Friendly(x - 3, y - 1, col) OrElse InAtari(x - 2, y, -col)) AndAlso (Friendly(x - 3, y + 1, col) OrElse InAtari(x - 2, y, -col)) Then Return True
            End If
            If Internal(x + 3, y) AndAlso ID(x + 3, y) = ch Then
                If (Friendly(x, y - 1, col) OrElse InAtari(x + 1, y, -col)) AndAlso (Friendly(x, y + 1, col) OrElse InAtari(x + 1, y, -col)) AndAlso Friendly(x + 1, y - 1, col) AndAlso empty(x + 1, y) AndAlso Friendly(x + 1, y + 1, col) AndAlso Friendly(x + 2, y + 1, col) AndAlso empty(x + 2, y) AndAlso Friendly(x + 2, y - 1, col) AndAlso (Friendly(x + 3, y - 1, col) OrElse InAtari(x + 2, y, -col)) AndAlso (Friendly(x + 3, y + 1, col) OrElse InAtari(x + 2, y, -col)) Then Return True
            End If
        End If

        Return False

    End Function

    Public Function funct(ch As Integer, what As String, Optional killed As Integer = 0) As Integer

        Dim i, j, k As Integer
        Dim p As Point

        ' liberties and eyes/special eyes/eyelikes are counted
        funct = 0
        Select Case what
            ' liberties
            Case "LS"
                For i = 0 To N1 : For j = 0 To N1
                        If board(i, j) = 0 Then
                            If i > 0 AndAlso ID(i - 1, j) = ch Then
                                funct += 1
                                If Not Chains(ch).liberty.Contains(New Point(i, j)) Then Chains(ch).liberty.Add(New Point(i, j))
                            ElseIf i < N1 AndAlso ID(i + 1, j) = ch Then
                                funct += 1
                                If Not Chains(ch).liberty.Contains(New Point(i, j)) Then Chains(ch).liberty.Add(New Point(i, j))
                            ElseIf j > 0 AndAlso ID(i, j - 1) = ch Then
                                funct += 1
                                If Not Chains(ch).liberty.Contains(New Point(i, j)) Then Chains(ch).liberty.Add(New Point(i, j))
                            ElseIf j < N1 AndAlso ID(i, j + 1) = ch Then
                                funct += 1
                                If Not Chains(ch).liberty.Contains(New Point(i, j)) Then Chains(ch).liberty.Add(New Point(i, j))
                            End If
                        End If
                    Next j
                Next i
            ' first type of eyes
            Case "E1"
                For Each p In Chains(ch).emptyNeighbourPoints
                    If eye1(p, ch) Then
                        funct += 1
                        If Not Chains(ch).eyes.Contains(p) Then Chains(ch).eyes.Add(p)
                    End If
                Next
            ' second type of eyes
            Case "E2"
                For Each p In Chains(ch).emptyNeighbourPoints
                    If eye2(p, ch) Then
                        funct += 1
                        If Not Chains(ch).eyes.Contains(p) Then Chains(ch).eyes.Add(p)
                    End If
                Next
            ' third type of eyes
            Case "E3"
                For Each p In Chains(ch).emptyNeighbourPoints
                    If eye3(p, ch, killed) Then
                        funct += 1
                        If Not Chains(ch).eyes.Contains(p) Then Chains(ch).eyes.Add(p)
                    End If
                Next
            ' special eyes
            Case "SE"
                For Each p In Chains(ch).emptyNeighbourPoints
                    If specialEye(p, ch) Then
                        If Not Chains(ch).eyes.Contains(p) Then
                            funct += 1
                            If Not Chains(ch).specialEyes.Contains(p) Then Chains(ch).specialEyes.Add(p)
                        End If
                    End If
                Next
            'eyelikes
            Case "EL"
                k = 0
                For Each p In Chains(ch).emptyNeighbourPoints
                    If eyelike(p, ch) Then
                        funct += 1
                        If Not Chains(ch).eyelikes.Contains(p) Then Chains(ch).eyelikes.Add(p)
                    End If
                Next
            Case Else
        End Select
        Return funct

    End Function

    Private Function HalfConnection(x As Integer, y As Integer, ch As Integer) As Integer

        ' is point (x,y) a possible extension for chain ch, albeit disruptable?
        Dim col As Integer = Chains(ch).colour
        HalfConnection = 0

        ' diagonal jump: one of the two side points must be empty
        If Internal(x + 1, y + 1) AndAlso ID(x + 1, y + 1) = ch Then
            If empty(x + 1, y) OrElse empty(x, y + 1) Then HalfConnection += 1
        End If
        If Internal(x - 1, y + 1) AndAlso ID(x - 1, y + 1) = ch Then
            If empty(x - 1, y) OrElse empty(x, y + 1) Then HalfConnection += 1
        End If
        If Internal(x + 1, y - 1) AndAlso ID(x + 1, y - 1) = ch Then
            If empty(x + 1, y) OrElse empty(x, y - 1) Then HalfConnection += 1
        End If
        If Internal(x - 1, y - 1) AndAlso ID(x - 1, y - 1) = ch Then
            If empty(x - 1, y) OrElse empty(x, y - 1) Then HalfConnection += 1
        End If
        ' one point jump: the middle point must be empty
        If Internal(x + 2, y) AndAlso ID(x + 2, y) = ch Then
            If empty(x + 1, y) Then HalfConnection += 1
        End If
        If Internal(x - 2, y) AndAlso ID(x - 2, y) = ch Then
            If empty(x - 1, y) Then HalfConnection += 1
        End If
        If Internal(x, y + 2) AndAlso ID(x, y + 2) = ch Then
            If empty(x, y + 1) Then HalfConnection += 1
        End If
        If Internal(x, y - 2) AndAlso ID(x, y - 2) = ch Then
            If empty(x, y - 1) Then HalfConnection += 1
        End If
        ' knight's jump: the two middle points must be empty, although the matter is complicated and the neighbours' status matter
        If Internal(x - 1, y + 2) AndAlso ID(x - 1, y + 2) = ch Then
            If (empty(x, y + 1) AndAlso Friendly(x, y + 2, col) AndAlso Not (enemy(x - 1, y + 1, col) AndAlso enemy(x - 1, y, col))) OrElse (empty(x, y + 1) AndAlso Friendly(x - 1, y + 1, col)) OrElse (empty(x - 1, y + 1) AndAlso Friendly(x - 1, y, col) AndAlso Not (enemy(x, y + 1, col) AndAlso enemy(x, y + 2, col))) OrElse (empty(x - 1, y + 1) AndAlso Friendly(x, y + 1, col)) Then HalfConnection += 1
        End If
        If Internal(x - 1, y - 2) AndAlso ID(x - 1, y - 2) = ch Then
            If (empty(x, y - 1) AndAlso Friendly(x, y - 2, col) AndAlso Not (enemy(x - 1, y - 1, col) AndAlso enemy(x - 1, y, col))) OrElse (empty(x, y - 1) AndAlso Friendly(x - 1, y - 1, col)) OrElse (empty(x - 1, y - 1) AndAlso Friendly(x - 1, y, col) AndAlso Not (enemy(x, y - 1, col) AndAlso enemy(x, y - 2, col))) OrElse (empty(x - 1, y - 1) AndAlso Friendly(x, y - 1, col) AndAlso Not (enemy(x, y - 1, col) AndAlso enemy(x, y - 2, col))) Then HalfConnection += 1
        End If
        If Internal(x + 1, y - 2) AndAlso ID(x + 1, y - 2) = ch Then
            If (empty(x, y - 1) AndAlso Friendly(x, y - 2, col) AndAlso Not (enemy(x + 1, y - 1, col) AndAlso enemy(x + 1, y, col))) OrElse (empty(x, y - 1) AndAlso Friendly(x + 1, y - 1, col)) OrElse (empty(x + 1, y - 1) AndAlso Friendly(x + 1, y, col) AndAlso Not (enemy(x, y - 1, col) AndAlso enemy(x, y - 2, col))) OrElse (empty(x + 1, y - 1) AndAlso Friendly(x, y - 1, col)) Then HalfConnection += 1
        End If
        If Internal(x + 1, y + 2) AndAlso ID(x + 1, y + 2) = ch Then
            If (empty(x, y + 1) AndAlso Friendly(x, y + 2, col) AndAlso Not (enemy(x + 1, y + 1, col) AndAlso enemy(x + 1, y, col))) OrElse (empty(x, y + 1) AndAlso Friendly(x + 1, y + 1, col)) OrElse (empty(x + 1, y + 1) AndAlso Friendly(x + 1, y, col) AndAlso Not (enemy(x, y + 1, col) AndAlso enemy(x, y + 2, col))) OrElse (empty(x + 1, y + 1) AndAlso Friendly(x, y + 1, col)) Then HalfConnection += 1
        End If
        If Internal(x + 2, y - 1) AndAlso ID(x + 2, y - 1) = ch Then
            If (empty(x + 1, y) AndAlso Friendly(x + 2, y, col) AndAlso Not (enemy(x + 1, y - 1, col) AndAlso enemy(x, y - 1, col))) OrElse (empty(x + 1, y) AndAlso Friendly(x + 1, y - 1, col)) OrElse (empty(x + 1, y - 1) AndAlso Friendly(x, y - 1, col) AndAlso Not (enemy(x + 1, y, col) AndAlso enemy(x + 2, y, col))) OrElse (empty(x + 1, y - 1) AndAlso Friendly(x + 1, y, col)) Then HalfConnection += 1
        End If
        If Internal(x + 2, y + 1) AndAlso ID(x + 2, y + 1) = ch Then
            If (empty(x + 1, y) AndAlso Friendly(x + 2, y, col) AndAlso Not (enemy(x + 1, y + 1, col) AndAlso enemy(x, y + 1, col))) OrElse (empty(x + 1, y) AndAlso Friendly(x + 1, y + 1, col)) OrElse (empty(x + 1, y + 1) AndAlso Friendly(x, y + 1, col) AndAlso Not (enemy(x + 1, y, col) AndAlso enemy(x + 2, y, col))) OrElse (empty(x + 1, y + 1) AndAlso Friendly(x + 1, y, col)) Then HalfConnection += 1
        End If
        If Internal(x - 2, y - 1) AndAlso ID(x - 2, y - 1) = ch Then
            If (empty(x - 1, y) AndAlso Friendly(x - 2, y, col) AndAlso Not (enemy(x - 1, y - 1, col) AndAlso enemy(x, y - 1, col))) OrElse (empty(x - 1, y) AndAlso Friendly(x - 1, y - 1, col)) OrElse (empty(x - 1, y - 1) AndAlso Friendly(x, y - 1, col) AndAlso Not (enemy(x - 1, y, col) AndAlso enemy(x - 2, y, col))) OrElse (empty(x - 1, y - 1) AndAlso Friendly(x - 1, y, col)) Then HalfConnection += 1
        End If
        If Internal(x - 2, y + 1) AndAlso ID(x - 2, y + 1) = ch Then
            If (empty(x - 1, y) AndAlso Friendly(x - 2, y, col) AndAlso Not (enemy(x - 1, y + 1, col) AndAlso enemy(x, y + 1, col))) OrElse (empty(x - 1, y) AndAlso Friendly(x - 1, y + 1, col)) OrElse (empty(x - 1, y + 1) AndAlso Friendly(x, y + 1, col) AndAlso Not (enemy(x - 1, y, col) AndAlso enemy(x - 2, y, col))) OrElse (empty(x - 1, y + 1) AndAlso Friendly(x - 1, y, col)) Then HalfConnection += 1
        End If

    End Function

    Private Function InAtari(x As Integer, y As Integer, col As Integer, Optional free As Boolean = True) As Boolean

        ' point p is checked for colour col: it is a point "in atari" if a stone of colour col, when put there, is captured at once
        Dim liberty1, liberty2 As Integer
        Dim temp(18, 18) As Integer
        liberty1 = 0
        liberty2 = 0

        If board(x, y) <> 0 Then Return False
        Array.Copy(board, temp, 361)
        board(x, y) = col
        liberty1 = GroupLib(x, y)
        Array.Copy(origBoard, board, 361)
        If board(x, y) = 0 Then
            board(x, y) = col
            liberty2 = GroupLib(x, y)
        Else
            liberty2 = 2
        End If
        Array.Copy(temp, board, 361)
        ' if "free" = false then the point is also checked in the original board (restoring dead strings): that could be useful when counting eyes
        InAtari = False
        Select Case free
            Case True : If liberty1 <= 1 Then Return True Else Return False
            Case False : If liberty1 <= 1 Or liberty2 <= 1 Then Return True Else Return False
        End Select

    End Function

    Private Function Internal(x As Integer, y As Integer) As Boolean

        ' verifies that point (x,y) is inside the goban
        Return x >= 0 AndAlso x <= N1 AndAlso y >= 0 AndAlso y <= N1

    End Function

    Private Function Killing(ch As Integer) As Boolean

        Dim good As Boolean
        Dim ch1 As Integer
        Dim eyes As Integer
        Dim meanx, meany As Double
        Dim pt1, pt2 As Point
        Dim l1, lpot, lpot1, lpot2 As Integer
        Dim z As Integer
        Dim p As Point

        ' is the chain ch a "stealing eyes" type? The well-known configurations are looked for
        Killing = False
        good = False

        ' one stone, two liberties
        If Chains(ch).size = 1 AndAlso groups(Chains(ch).group).size = 1 AndAlso Chains(ch).liberty.Count = 2 Then
            good = True
            Dim pts As New ArrayList
            pts = CType(Chains(ch).liberty.Clone, ArrayList)
            lpot1 = 0
            ' the two liberties are filled, one by one, and the liberties of the new chain are counted, then summed
            For Each pt1 In pts
                board(pt1.X, pt1.Y) = Chains(ch).colour
                l1 = GroupLib(pt1.X, pt1.Y)
                lpot1 += l1
                ' if filling one of the liberties doesn't change the overall number (2) the check is repeated on the new string
                If l1 = 2 Then
                    ID(pt1.X, pt1.Y) = ch
                    Chains(ch).liberty.Clear()
                    funct(ch, "LS")
                    lpot2 = 0
                    For Each pt2 In Chains(ch).liberty
                        board(pt2.X, pt2.Y) = Chains(ch).colour
                        lpot2 += GroupLib(pt2.X, pt2.Y)
                        board(pt2.X, pt2.Y) = 0
                    Next
                    ' if the sum of the new liberties found in the secondary control is more than 2, the original chain is not a "stealing eyes" type
                    If lpot2 > 2 Then good = False
                    ID(pt1.X, pt1.Y) = 0
                End If
                board(pt1.X, pt1.Y) = 0
            Next
            ' if the sum of the new liberties found in the primary control is more than 4, the chain is not a "stealing eyes" type
            If lpot1 > 4 Then good = False
        End If
        ' one stone, three liberties; there is no secondary control
        If Chains(ch).size = 1 AndAlso groups(Chains(ch).group).size = 1 AndAlso Chains(ch).liberty.Count = 3 Then
            lpot = 0
            For Each p In Chains(ch).liberty
                board(p.X, p.Y) = Chains(ch).colour
                lpot += GroupLib(p.X, p.Y)
                board(p.X, p.Y) = 0
            Next
            ' if the sum of the new liberties is 6, there is a "pyramid four"; if the sum is 8, we get a "bulky five"
            ' due configurazioni uccidono (tridente, trapezio)
            If lpot = 6 Or lpot = 8 Then good = True
        End If
        ' one stone, four liberties, no secondary control
        If Chains(ch).size = 1 AndAlso groups(Chains(ch).group).size = 1 AndAlso Chains(ch).liberty.Count = 4 Then
            lpot = 0
            For Each p In Chains(ch).liberty
                board(p.X, p.Y) = Chains(ch).colour
                lpot += GroupLib(p.X, p.Y)
                board(p.X, p.Y) = 0
            Next
            ' if the sum of the new liberties is 12, there is a "crossed five"; if it is 14, we get the "rabbitty six"
            If lpot = 12 Or lpot = 14 Then good = True
        End If
        ' two stones, two liberty kill if the liberties are "in atari" points for the chain
        If Chains(ch).size = 2 AndAlso groups(Chains(ch).group).size = 1 AndAlso Chains(ch).liberty.Count = 2 Then
            good = True
            For Each p In Chains(ch).liberty
                If Not InAtari(p.X, p.Y, Chains(ch).colour) Then good = False
            Next
        End If
        ' three stones, one liberty of course kill
        If Chains(ch).size = 3 AndAlso groups(Chains(ch).group).size = 1 AndAlso Chains(ch).liberty.Count = 1 Then
            good = True
            For Each p In Chains(ch).liberty
                If Not InAtari(p.X, p.Y, Chains(ch).colour) Then good = False
            Next
        End If
        ' four stones in a square, two liberties: always kill
        If Chains(ch).size = 4 AndAlso groups(Chains(ch).group).size = 1 AndAlso Chains(ch).liberty.Count = 2 Then
            good = True
            meanx = 0 : meany = 0
            For z = 1 To 4
                meanx += Chains(ch).p(z).X
                meany += Chains(ch).p(z).Y
            Next z
            meanx = meanx / 4
            meany = meany / 4
            For z = 1 To 4
                If Math.Abs(meanx - Chains(ch).p(z).X) <> 0.5 Then good = False
                If Math.Abs(meany - Chains(ch).p(z).Y) <> 0.5 Then good = False
            Next
            If good Then
                For Each p In Chains(ch).liberty
                    If Not InAtari(p.X, p.Y, Chains(ch).colour) Then good = False
                Next
            End If
        End If

        If good Then
            ' are there eyes in the neighbour chains?
            eyes = 0
            For Each p In Chains(ch).neighbourPoints
                ch1 = ID(p.X, p.Y)
                If ch1 <> 0 And ch1 <> ch Then
                    ' if the killing chain looks like dying before its neighbours, it is worth one eye for them
                    If Chains(ch1).status < Chains(ch).status Then
                        stringAnalysis(ch1, False, ch)
                        eyes += Chains(ch1).eyes.Count
                    End If
                    eyes += groups(Chains(ch1).group).eyes.Count
                End If
            Next
            ' so neighbour chains with no eyes are dead (with one eye they live instead)
            If eyes = 0 Then Killing = True
            For Each p In Chains(ch).liberty
                ' z = 0 means the "stealing eyes" chain is black, also worth an eye for white neighbour chains; z = 2 means the opposite
                z = 2 - (Chains(ch).colour + 1)
                If Not killingeyes(z).Contains(p) Then killingeyes(z).Add(p)
            Next
        End If

    End Function

    Private Function KO(x As Integer, y As Integer) As Integer

        Dim col1, col2, col3, col4 As Integer
        Dim colour As Integer
        Dim p, v As Integer

        ' is the point (x,y) a KO, and which colour will fill it? 0 = no KO; 1 = Black fills the KO; -1 = White fills the KO

        If board(x, y) <> 0 Then Return board(x, y)
        ' First condition: the points surrounding the possibile KO are checked, and its colour (if the KO possibly exists) is determined
        If Internal(x + 1, y) Then col1 = board(x + 1, y) Else col1 = 2
        If Internal(x - 1, y) Then col2 = board(x - 1, y) Else col2 = 2
        If Internal(x, y + 1) Then col3 = board(x, y + 1) Else col3 = 2
        If Internal(x, y - 1) Then col4 = board(x, y - 1) Else col4 = 2
        Select Case minimum(col1, col2, col3, col4)
            ' all of them are white
            Case -1 : If (col1 = -1 Or col1 = 2) And (col2 = -1 Or col2 = 2) And (col3 = -1 Or col3 = 2) And (col4 = -1 Or col4 = 2) Then colour = -1 Else Return 0
            ' not the same colour, thus no KO
            Case 0 : Return 0
            ' all of them are black
            Case 1 : colour = 1
        End Select

        ' second condition: there are four possible configuration, depending on KO's orientation, and every one of them must be checked

        ' first configuration
        p = 0 : v = 0
        If Internal(x - 1, y - 1) AndAlso board(x - 1, y - 1) = -colour Then p += 1
        If Not Internal(x - 1, y - 1) Then v += 1
        If Internal(x, y - 2) AndAlso board(x, y - 2) = -colour Then p += 1
        If Not Internal(x, y - 2) Then v += 1
        If Internal(x + 1, y - 1) AndAlso board(x + 1, y - 1) = -colour Then p += 1
        If Not Internal(x + 1, y - 1) Then v += 1
        If p + v = 3 And p >= 1 Then Return colour
        ' second configuration
        p = 0 : v = 0
        If Internal(x + 1, y - 1) AndAlso board(x + 1, y - 1) = -colour Then p += 1
        If Not Internal(x + 1, y - 1) Then v += 1
        If Internal(x + 2, y) AndAlso board(x + 2, y) = -colour Then p += 1
        If Not Internal(x + 2, y) Then v += 1
        If Internal(x + 1, y + 1) AndAlso board(x + 1, y + 1) = -colour Then p += 1
        If Not Internal(x + 1, y + 1) Then v += 1
        If p + v = 3 And p >= 1 Then Return colour
        ' third configuration
        p = 0 : v = 0
        If Internal(x - 1, y + 1) AndAlso board(x - 1, y + 1) = -colour Then p += 1
        If Not Internal(x - 1, y + 1) Then v += 1
        If Internal(x, y + 2) AndAlso board(x, y + 2) = -colour Then p += 1
        If Not Internal(x, y + 2) Then v += 1
        If Internal(x + 1, y + 1) AndAlso board(x + 1, y + 1) = -colour Then p += 1
        If Not Internal(x + 1, y + 1) Then v += 1
        If p + v = 3 And p >= 1 Then Return colour
        ' fourth configuration
        p = 0 : v = 0
        If Internal(x - 1, y - 1) AndAlso board(x - 1, y - 1) = -colour Then p += 1
        If Not Internal(x - 1, y - 1) Then v += 1
        If Internal(x - 2, y) AndAlso board(x - 2, y) = -colour Then p += 1
        If Not Internal(x - 2, y) Then v += 1
        If Internal(x - 1, y + 1) AndAlso board(x - 1, y + 1) = -colour Then p += 1
        If Not Internal(x - 1, y + 1) Then v += 1
        If p + v = 3 And p >= 1 Then Return colour

        Return 0

    End Function

    Private Function GroupLib(ByVal X As Integer, ByVal Y As Integer, Optional ByVal GLinit As Boolean = True) As Integer

        Dim i As Integer
        Dim j As Integer

        ' given the point (x,y), the stone occupying it, and the chain including the stone, how many liberties this chain has got?
        ' the function is recursive, hence the parameter GLinit (= false when the function is called recursively)
        If GLinit Then
            GLColour = board(X, Y)
            GLTotal = 0
            For i = 0 To N1
                For j = 0 To N1
                    tempGroup(i, j) = 0
                Next j
            Next i
            If GLColour = 0 Then Return -1
        End If

        Select Case board(X, Y)
            Case GLColour
                If tempGroup(X, Y) = 0 Then
                    tempGroup(X, Y) = 1
                    If Y > 0 Then GroupLib(X, Y - 1, False)
                    If Y < N1 Then GroupLib(X, Y + 1, False)
                    If X > 0 Then GroupLib(X - 1, Y, False)
                    If X < N1 Then GroupLib(X + 1, Y, False)
                End If
            Case 0
                If tempGroup(X, Y) = 0 Then
                    GLTotal += 1
                    tempGroup(X, Y) = 2
                End If
        End Select
        GroupLib = GLTotal

    End Function

    Private Function minimum(a As Integer, b As Integer, c As Integer, d As Integer) As Integer

        ' returns minimum among four numbers
        Dim e, f As Integer

        If a < b Then e = a Else e = b
        If c < d Then f = c Else f = d
        If e < f Then Return e Else Return f

    End Function

    Private Function enemy(x As Integer, y As Integer, col As Integer) As Boolean

        ' true if point (x,y) is occupied by a stone of enemy colour (the opposite of "col")
        If Internal(x, y) AndAlso board(x, y) = -col Then Return True Else Return False

    End Function

    Private Function eye1(p As Point, ch As Integer) As Boolean

        ' first type of eye, when point p is surrounded by stones belonging to the same chain
        Dim count As Integer = 0
        If Not Internal(p.X, p.Y) Then Return False

        If (p.Y > 0 AndAlso ID(p.X, p.Y - 1) = ch) OrElse p.Y = 0 Then count += 1
        If (p.X < N1 AndAlso ID(p.X + 1, p.Y) = ch) OrElse p.X = N1 Then count += 1
        If (p.Y < N1 AndAlso ID(p.X, p.Y + 1) = ch) OrElse p.Y = N1 Then count += 1
        If (p.X > 0 AndAlso ID(p.X - 1, p.Y) = ch) OrElse p.X = 0 Then count += 1

        If count = 4 Then Return True Else Return False

    End Function

    Private Function eye2(p As Point, ch As Integer) As Boolean

        ' second type of eye
        Dim count As Integer = 0
        Dim emptyangles As Integer = 0
        Dim r As Integer
        Dim cx, cy As Integer
        Dim j As Integer = 0
        Dim x As Integer = p.X
        Dim y As Integer = p.Y
        ' not a second type of eye if already a first type, as well as there is another eye in some neighbour point
        If eye1(p, ch) Then Return False : Exit Function
        For r = 1 To 4
            If Chains(ch).eyes.Contains(veryClose(r, p.X, p.Y)) Then Return False
        Next r
        ' the 8 neighbour points are checked
        If Chains(ch).emptyNeighbourPoints.Contains(p) Then
            ' point is on the border
            If x = 0 OrElse x = N1 OrElse y = 0 OrElse y = N1 Then count = 3
            ' point is on the corner
            If (x = 0 AndAlso (y = 0 OrElse y = N1)) OrElse (x = N1 AndAlso (y = 0 OrElse y = N1)) Then count = 5
            For cx = x - 1 To x + 1
                For cy = y - 1 To y + 1
                    If Internal(cx, cy) AndAlso Not (x = cx And y = cy) Then
                        ' one of the neighbour points is occupied by an enemy stone: no eye
                        If board(cx, cy) = -Chains(ch).colour Then Return False
                        ' either one of them is occupied by a friendly stone or is already an eye
                        If board(cx, cy) = Chains(ch).colour Then count += 1
                        If board(cx, cy) = 0 Then
                            If groups(Chains(ch).group).eyes.Contains(New Point(cx, cy)) Then
                                count += 1
                            Else
                                If Math.Abs(cx - x) = 1 AndAlso Math.Abs(cy - y) = 1 Then emptyangles += 1
                            End If
                        End If
                    End If
                Next
            Next

        End If

        ' it's an eye if there are no enemy stones in the neighbour points, at least 6 friendly stones, and at most two angles are empty
        If count + emptyangles = 8 And count >= 6 Then Return True Else Return False

    End Function

    Private Function eye3(p As Point, ch As Integer, Optional killed As Integer = 0) As Boolean

        ' third type of eye
        Dim count As Integer = 0
        Dim enemyangles As Integer = 0
        Dim x As Integer = p.X
        Dim y As Integer = p.Y
        Dim cx, cy As Integer
        Dim r, s As Integer
        Dim contiguous As Boolean = False

        ' special control for third type of eye
        ' stones in a dead chain count like friendly stones, on condition they are NOT contiguous to the possible eye (if they are in the angles it's OK)
        If killed <> 0 Then
            For r = 0 To Chains(killed).emptyNeighbourPoints.Count - 1
                If p = CType(Chains(killed).emptyNeighbourPoints(r), Point) Then
                    For s = 1 To Chains(killed).size
                        If p.X = Chains(killed).p(s).X Or p.Y = Chains(killed).p(s).Y Then contiguous = True
                    Next s
                End If
            Next r
            If contiguous Then Return False
        End If
        ' not a third type of eye if already a first/second type, as well if there is another eye in some neighbour point
        If eye1(p, ch) OrElse eye2(p, ch) Then Return False
        For r = 1 To 4
            If Chains(ch).eyes.Contains(veryClose(r, p.X, p.Y)) Then Return False
        Next r
        ' point is on the border
        If x = 0 OrElse x = N1 OrElse y = 0 OrElse y = N1 Then count = 3
        ' point is on the corner
        If (x = 0 AndAlso (y = 0 OrElse y = N1)) OrElse (x = N1 AndAlso (y = 0 OrElse y = N1)) Then count = 5
        For cx = x - 1 To x + 1
            For cy = y - 1 To y + 1
                If Internal(cx, cy) AndAlso Not (x = cx AndAlso y = cy) Then
                    ' no eye: an enemy stone is in one of the neighbour points, and not in an angle
                    If board(cx, cy) = -Chains(ch).colour AndAlso (cx = x Or cy = y) Then Return False
                    ' no eye: we are on a border and there is an enemy stone in a neighbour point
                    If board(cx, cy) = -Chains(ch).colour AndAlso (x = 0 OrElse x = N1 OrElse y = 0 OrElse y = N1) Then Return False
                    ' there is an enemy stone in one of the neighbour points, but it is an angle
                    If board(cx, cy) = -Chains(ch).colour AndAlso (cx <> x AndAlso cy <> y) AndAlso (ID(cx, cy) <> killed Or killed = 0) Then
                        enemyangles += 1
                        ' the neighbout point is occupied by a friendly stone/an eye/is point "in atari"/there is an enemy stone but surely dead
                    ElseIf board(cx, cy) = Chains(ch).colour OrElse groups(Chains(ch).group).eyes.Contains(New Point(cx, cy)) OrElse InAtari(cx, cy, -Chains(ch).colour, False) OrElse (ID(cx, cy) = killed And killed <> 0) Then
                        count += 1
                    End If
                End If
            Next
        Next

        ' it's an eye if at least 7 out of 8 neighbour points are friendly (or similar) and the last one is an angle
        If ((count = 7 AndAlso enemyangles = 1) OrElse (count = 8)) Then Return True Else Return False

    End Function

    Private Function falseEye(p As Point, gr As Integer) As Boolean

        Dim count As Integer = 0
        Dim enemyangles As Integer = 0
        Dim x As Integer = p.X
        Dim y As Integer = p.Y
        Dim cx, cy As Integer

        ' false eye (useful in sekis): 6 out of 8 neighbour points must be friendly, the remaining ones must be angles and enemies (on the border, 7 and 1 respectively)
        ' point is on the border
        If x = 0 OrElse x = N1 OrElse y = 0 OrElse y = N1 Then count = 3
        ' point is on the corner
        If (x = 0 AndAlso (y = 0 OrElse y = N1)) OrElse (x = N1 AndAlso (y = 0 OrElse y = N1)) Then count = 5
        For cx = x - 1 To x + 1
            For cy = y - 1 To y + 1
                If Internal(cx, cy) AndAlso Not (x = cx And y = cy) Then
                    If board(cx, cy) = -groups(gr).colour AndAlso (cx <> x And cy <> y) Then enemyangles += 1
                    If board(cx, cy) = groups(gr).colour OrElse groups(gr).eyes.Contains(New Point(cx, cy)) OrElse groups(gr).specialEyes.Contains(New Point(cx, cy)) OrElse InAtari(cx, cy, -groups(gr).colour) Then count += 1
                End If
            Next
        Next

        falseEye = False
        If count = 6 And enemyangles = 2 Then falseEye = True
        If x = 0 OrElse x = N1 OrElse y = 0 OrElse y = N1 Then
            If count = 7 And enemyangles = 1 Then falseEye = True
        End If

    End Function

    Private Function specialEye(p As Point, ch As Integer) As Boolean

        Dim count As Integer = 0
        Dim enemyangles As Integer = 0
        Dim x As Integer = p.X
        Dim y As Integer = p.Y
        Dim cx, cy As Integer

        ' special eye (if point p is not an eye already)
        If eye1(p, ch) OrElse eye2(p, ch) OrElse eye3(p, ch) Then Return False
        ' point on the border
        If x = 0 OrElse x = N1 OrElse y = 0 OrElse y = N1 Then count = 3
        ' point on the corner
        If (x = 0 AndAlso (y = 0 OrElse y = N1)) OrElse (x = N1 AndAlso (y = 0 OrElse y = N1)) Then count = 5
        For cx = x - 1 To x + 1
            For cy = y - 1 To y + 1
                If Internal(cx, cy) AndAlso Not (x = cx And y = cy) Then
                    ' not a special eye: one of the neighbour points is enemy and not angle
                    If board(cx, cy) = -Chains(ch).colour AndAlso (cx = x Or cy = y) Then Return False
                    ' not a special eye_ on of the neighbour points is enemy and we are on the border
                    If board(cx, cy) = -Chains(ch).colour AndAlso (x = 0 Or x = N1 Or y = 0 Or y = N1) Then Return False
                    ' enemy stone in and angle
                    If board(cx, cy) = -Chains(ch).colour AndAlso (cx <> x And cy <> y) Then enemyangles += 1
                    ' neighbour point is friendly/eye/special eye/point in Atari
                    If board(cx, cy) = Chains(ch).colour OrElse Chains(ch).eyes.Contains(New Point(cx, cy)) OrElse Chains(ch).specialEyes.Contains(New Point(cx, cy)) OrElse InAtari(cx, cy, -Chains(ch).colour) Then count += 1
                End If
            Next
        Next

        ' special eye if at least 6 out 8 neighbour points are friendly and at most one angle is enemy
        If count >= 6 And enemyangles < 2 Then Return True Else Return False

    End Function

    Private Function KOfilling() As Integer

        Dim i, j As Integer
        Dim position As Point
        Dim KOPOS As Integer
        Dim Taboo As Boolean
        Dim ch As Integer
        Dim z As Integer = 0

        For i = 0 To N1
            For j = 0 To N1
                If board(i, j) = 0 Then
                    KOPOS = KO(i, j)
                    Taboo = InAtari(i, j, KOPOS)
                    ' there is a KO in point (i,j)
                    If KOPOS <> 0 Then
                        ' it is connected
                        board(i, j) = KOPOS
                        z += 1
                        KOPoint(z).X = i * KOPOS : KOPoint(z).Y = j * KOPOS
                        If Taboo Then
                            StringNumber = chainDefine()
                            ch = ID(i, j)
                            If funct(ch, "LS") > 0 Then
                                position = CType(Chains(ch).liberty.Item(0), Point)
                                ' after connection, does the chain remain in atari?
                                ' were that the case, let's try an inner connection
                                board(position.X, position.Y) = KOPOS
                                z += 1
                                KOPoint(z).X = position.X * KOPOS : KOPoint(z).Y = position.Y * KOPOS
                                StringNumber = chainDefine()
                                ' were this inner connection not possible...
                                ' se la nuova connessione non è possibile ...
                                If Not InAtari(position.X, position.Y, KOPOS) Then
                                    ' ... the KO is connected by the opposite colour...
                                    board(i, j) = -KOPOS
                                    checkCaptures(i, j)
                                    StringNumber = chainDefine()
                                    ' ... which is compensated by means of one point of territory
                                    bonus(ID(position.X, position.Y)) = True
                                End If
                            End If
                        End If
                    End If
                End If
            Next j
        Next i

        Return z

    End Function

    Private Function removeAtari(s As Integer, l As Integer) As Point

        Dim colour As Integer = Chains(s).colour
        Dim i, j As Integer
        Dim damebackup(18, 18) As Integer

        removeAtari = New Point(99, 99)
        Array.Copy(board, damebackup, 361)

        ' when a chain remains in atari after dame filling, we may look for dead chains to capture at once in order to increase liberties' number
        For i = 0 To N1
            For j = 0 To N1
                If board(i, j) = 0 AndAlso Math.Sign(intensity(i, j)) = colour Then
                    board(i, j) = colour
                    checkCaptures(i, j)
                    If (funct(s, "LS") > 1 And l = 1) OrElse (funct(s, "LS") >= 1 And l = 0) Then
                        removeAtari = New Point(i, j)
                        Array.Copy(damebackup, board, 361)
                        Exit Function
                    End If
                    Array.Copy(damebackup, board, 361)
                End If
            Next j
        Next i

    End Function

    Private Sub chainRemove(s As Integer)

        Dim i, j, k As Integer
        ' removal (from the virtual borad) of a dead chain
        For k = 1 To Chains(s).size
            i = Chains(s).p(k).X
            j = Chains(s).p(k).Y
            board(i, j) = 0
            ID(i, j) = 0
            IDGR(i, j) = 0
#If GUI Then
            If shows Then Main.plot(removing, i, j, Color.Red)
#End If
        Next
        Chains(s).size = 0
        Chains(s).status = 0

    End Sub

    Private Function eyelike(p As Point, ch As Integer) As Boolean

        Dim count As Integer = 0
        Dim spaces As Integer = 0
        Dim x As Integer = p.X
        Dim y As Integer = p.Y
        Dim cx, cy As Integer

        ' eyelike (if point is not eye or special eye already)
        If eye1(p, ch) Or eye2(p, ch) Or eye3(p, ch) Or specialEye(p, ch) Then Return False

        ' point on the border
        If x = 0 OrElse x = N1 OrElse y = 0 OrElse y = N1 Then count = 3
        ' point on the corner
        If (x = 0 AndAlso (y = 0 OrElse y = N1)) OrElse (x = N1 AndAlso (y = 0 OrElse y = N1)) Then count = 5
        For cx = x - 1 To x + 1
            For cy = y - 1 To y + 1
                If Internal(cx, cy) AndAlso Not (x = cx And y = cy) Then
                    ' no eyelike: one the neighbour points is enemy
                    If board(cx, cy) = -Chains(ch).colour Then Return False
                    ' neighbour point is friendly/eye/special eye/point in atari/occupied by a dead enemy stone
                    If board(cx, cy) = Chains(ch).colour OrElse Chains(ch).eyes.Contains(New Point(cx, cy)) OrElse Chains(ch).specialEyes.Contains(New Point(cx, cy)) OrElse InAtari(cx, cy, -Chains(ch).colour) OrElse (board(cx, cy) = 0 And origBoard(cx, cy) = -Chains(ch).colour) Then count += 1
                    If board(cx, cy) = 0 Then spaces += 1
                End If
            Next
        Next
        ' eyelikes if at least 5 out of 8 neighbour points are friendly, the other ones are empty
        If count >= 5 And count + spaces >= 8 Then Return True Else Return False

    End Function

    Private Function veryClose(r As Integer, i As Integer, j As Integer) As Point

        ' according to r it returns one of the four contiguous points to point (i,j): the points above/under/on the right/on the left of point (i,j)
        Select Case r
            Case 1 : veryClose = New Point(i, j - 1)
            Case 2 : veryClose = New Point(i + 1, j)
            Case 3 : veryClose = New Point(i, j + 1)
            Case 4 : veryClose = New Point(i - 1, j)
        End Select

    End Function

    Private Function empty(x As Integer, y As Integer) As Boolean

        ' point (x,y) is empty if empty (of course) or outside the goban
        If (Internal(x, y) AndAlso board(x, y) = 0) OrElse Not Internal(x, y) Then Return True Else Return False

    End Function

End Module