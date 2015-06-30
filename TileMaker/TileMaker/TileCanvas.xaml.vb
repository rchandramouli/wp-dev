Imports System
Imports System.Threading
Imports System.Windows.Controls
Imports Microsoft.Phone.Controls
Imports Microsoft.Phone.Shell
Imports System.Windows.Media.Imaging
Imports Microsoft.Phone.PictureDecoder

Public Class GlobalVars
    Public Shared bInitialized As Boolean

    Public Shared gBmp As WriteableBitmap
    Public Shared gRows As Integer, gCols As Integer
    Public Shared gRowspan As Integer, gColspan As Integer
    Public Shared gInvert As Boolean

    ' MiP maps, various sizes, based on the map mode.
    '
    ' The maps are initially loaded based on the given image
    ' and trimmed/scaled (if required). Before drawing the
    ' image the map is loaded to gTileMiPMap and 
    '
    Public Shared gTileMiPMapX1(1, 1, 1, 1) As Integer
    Public Shared gTileMiPMapX2(1, 1, 1, 1) As Integer
    Public Shared gTileMiPMapX4(1, 1, 1, 1) As Integer
    Public Shared gTileMipMapX8(1, 1, 1, 1) As Integer
End Class

Partial Public Class TileCanvas
    Inherits PhoneApplicationPage

    Public Sub New()
        InitializeComponent()

        imgCanvas.Height = Application.Current.Host.Content.ActualHeight
        imgCanvas.Width = Application.Current.Host.Content.ActualWidth

        If GlobalVars.bInitialized = False Then
            GlobalVars.bInitialized = True
            GlobalVars.gBmp = New WriteableBitmap(imgCanvas.Width, imgCanvas.Height)
        End If
    End Sub

    Private Function GetIndexFromMap(map As Integer) As Integer
        ' X1/X2 variants
        If map = TileModeTypes.Normal Then
            GetIndexFromMap = 0
            Exit Function
        ElseIf map = TileModeTypes.Mirror Then
            GetIndexFromMap = 1
            Exit Function
        ElseIf map = TileModeTypes.RotateClock Then
            GetIndexFromMap = 2
            Exit Function
        End If

        ' X4 variants
        If map = TileModeTypes.MirrorRotateClock Then
            GetIndexFromMap = 0
            Exit Function
        ElseIf map = TileModeTypes.RotateClockMirror Then
            GetIndexFromMap = 1
            Exit Function
        End If

        GetIndexFromMap = 0
    End Function

    Public Sub RedimAllMiPMaps()
        ' X1: normal, invert
        ReDim GlobalVars.gTileMiPMapX1(1, 2, GlobalVars.gRows - 1, GlobalVars.gCols - 1)

        ' X2: normal, mirror, rotate
        ReDim GlobalVars.gTileMiPMapX2(3, 4, GlobalVars.gRows - 1, GlobalVars.gCols - 1)

        ' X4: mirror-rotate, rotate-mirror
        ReDim GlobalVars.gTileMiPMapX4(2, 16, GlobalVars.gRows - 1, GlobalVars.gCols - 1)
    End Sub

    '#
    '# Build a X2 scaled MiPMap from the X1 MiPMap normally. Do a copy from (r, c) to
    '# (r, c) of all 4 squares.
    '#
    Public Sub BuildMiPMap_Normal()
        Dim r As Integer, c As Integer, inv As Integer
        Dim rsize As Integer, csize As Integer, idx As Integer

        rsize = GlobalVars.gRows
        csize = GlobalVars.gCols

        idx = GetIndexFromMap(TileModeTypes.Normal)

        If GlobalVars.gInvert = True Then
            inv = 1
        Else
            inv = 0
        End If

        For r = 0 To rsize - 1
            For c = 0 To csize - 1
                GlobalVars.gTileMiPMapX2(idx, 0, r, c) = GlobalVars.gTileMiPMapX1(0, 0, r, c)
                GlobalVars.gTileMiPMapX2(idx, 1, r, c) = GlobalVars.gTileMiPMapX1(0, inv, r, c)
                GlobalVars.gTileMiPMapX2(idx, 2, r, c) = GlobalVars.gTileMiPMapX1(0, inv, r, c)
                GlobalVars.gTileMiPMapX2(idx, 3, r, c) = GlobalVars.gTileMiPMapX1(0, 0, r, c)
            Next
        Next
    End Sub

    '#
    '# Build a X2 scaled MiPMap from the X1 MiPMap with Horizontal/Vertical mirror. Do a
    '# copy from (r, c) to: (r, max-c), (max-r, c), (max-r, max-c)
    '#
    Public Sub BuildMiPMap_Mirror()
        Dim r As Integer, c As Integer, inv As Integer
        Dim rsize As Integer, csize As Integer, idx As Integer

        rsize = GlobalVars.gRows
        csize = GlobalVars.gCols

        idx = GetIndexFromMap(TileModeTypes.Mirror)

        If GlobalVars.gInvert = True Then
            inv = 1
        Else
            inv = 0
        End If

        For r = 0 To rsize - 1
            For c = 0 To csize - 1
                GlobalVars.gTileMiPMapX2(idx, 0, r, c) = GlobalVars.gTileMiPMapX1(0, 0, r, c)
                GlobalVars.gTileMiPMapX2(idx, 1, r, csize - 1 - c) = GlobalVars.gTileMiPMapX1(0, inv, r, c)
                GlobalVars.gTileMiPMapX2(idx, 2, rsize - 1 - r, c) = GlobalVars.gTileMiPMapX1(0, inv, r, c)
                GlobalVars.gTileMiPMapX2(idx, 3, rsize - 1 - r, csize - 1 - c) = GlobalVars.gTileMiPMapX1(0, 0, r, c)
            Next
        Next
    End Sub

    '#
    '# Build a X2 scaled MiPMap from the X1 MiPMap with Clockwise Rotation. Do a
    '# copy from (r, c) to: (c, max-r), (max-c, r), (max-r, max-c)
    '#
    Public Sub BuildMiPMap_RotateClock()
        Dim r As Integer, c As Integer, inv As Integer
        Dim rsize As Integer, csize As Integer, idx As Integer

        rsize = GlobalVars.gRows
        csize = GlobalVars.gCols

        idx = GetIndexFromMap(TileModeTypes.RotateClock)

        If GlobalVars.gInvert = True Then
            inv = 1
        Else
            inv = 0
        End If

        For r = 0 To rsize - 1
            For c = 0 To csize - 1
                GlobalVars.gTileMiPMapX2(idx, 0, r, c) = GlobalVars.gTileMiPMapX1(0, 0, r, c)
                GlobalVars.gTileMiPMapX2(idx, 1, r, c) = GlobalVars.gTileMiPMapX1(0, inv, csize - 1 - c, r)
                GlobalVars.gTileMiPMapX2(idx, 2, r, c) = GlobalVars.gTileMiPMapX1(0, 0, c, rsize - 1 - r)
                GlobalVars.gTileMiPMapX2(idx, 3, r, c) = GlobalVars.gTileMiPMapX1(0, inv, rsize - 1 - r, csize - 1 - c)

                'GlobalVars.gTileMiPMapX2(idx, 0, r, c) = GlobalVars.gTileMiPMapX1(0, 0, r, c)
                'GlobalVars.gTileMiPMapX2(idx, 1, r, c) = GlobalVars.gTileMiPMapX1(0, inv, c, r)
                'GlobalVars.gTileMiPMapX2(idx, 2, r, c) = GlobalVars.gTileMiPMapX1(0, inv, csize - 1 - c, rsize - 1 - r)
                'GlobalVars.gTileMiPMapX2(idx, 3, r, c) = GlobalVars.gTileMiPMapX1(0, 0, rsize - 1 - r, csize - 1 - c)
            Next
        Next
    End Sub

    '#
    '# Build a X4 scaled MiPMap from the X1 MiPMap with Horizontal/Vertical mirror and 
    '# then a Clockwise rotation of the resulting X2 MiPMap. Do following two stage operation:
    '#
    '#     1. Copy from (r, c) to: (r, max-c), (max-r, c), (max-r, max-c)
    '#     2. Copy from (r, c) to: (c, max-r), (max-c, r), (max-r, max-c)
    '#
    Public Sub BuildMiPMap_MirrorRotateClock()
        Dim r As Integer, c As Integer, inv As Integer
        Dim rsize As Integer, csize As Integer, idx As Integer

        rsize = GlobalVars.gRows
        csize = GlobalVars.gCols

        idx = GetIndexFromMap(TileModeTypes.MirrorRotateClock)

        If GlobalVars.gInvert = True Then
            inv = 1
        Else
            inv = 0
        End If

        ' Rotate-clockwise involves SWAP + FLIP-COLUMN operation. A point at (r, c) 
        ' moves to (c, max-r). Mirror operation involves in FLIP-COLUMN / FLIP-ROW
        ' operation.
        '
        ' With | A  B |
        '      | C  D | as a result of mirror operation, the new map with rotate operation
        ' gives,
        '
        '      | A  B  C  A |
        '      | C  D  D  B |
        '      | B  D  D  C |
        '      | A  C  B  A |,  the row/column transform of the elements w.r.t the X1 maps are,
        '
        '       (r, c)       (r, max-c)       (c, r)       (max-c, r)
        '       (max-r, c)   (max-r, max-c)   (c, max-r)   (max-c, max-r)
        '       (c, r)       (max-c, r)       (r, c)       (r, max-c)
        '       (c, max-r)   (max-c, max-r)   (max-r, c)   (max-r, max-c)
        '
        For r = 0 To rsize - 1
            For c = 0 To csize - 1
                GlobalVars.gTileMiPMapX4(idx, 0, r, c) = GlobalVars.gTileMiPMapX1(0, 0, r, c)
                GlobalVars.gTileMiPMapX4(idx, 1, r, c) = GlobalVars.gTileMiPMapX1(0, inv, r, csize - 1 - c)
                GlobalVars.gTileMiPMapX4(idx, 2, r, c) = GlobalVars.gTileMiPMapX1(0, 0, c, r)
                GlobalVars.gTileMiPMapX4(idx, 3, r, c) = GlobalVars.gTileMiPMapX1(0, inv, csize - 1 - c, r)
                GlobalVars.gTileMiPMapX4(idx, 4, r, c) = GlobalVars.gTileMiPMapX1(0, inv, rsize - 1 - r, c)
                GlobalVars.gTileMiPMapX4(idx, 5, r, c) = GlobalVars.gTileMiPMapX1(0, 0, rsize - 1 - r, csize - 1 - c)
                GlobalVars.gTileMiPMapX4(idx, 6, r, c) = GlobalVars.gTileMiPMapX1(0, inv, c, rsize - 1 - r)
                GlobalVars.gTileMiPMapX4(idx, 7, r, c) = GlobalVars.gTileMiPMapX1(0, 0, csize - 1 - c, rsize - 1 - r)
                GlobalVars.gTileMiPMapX4(idx, 8, r, c) = GlobalVars.gTileMiPMapX1(0, 0, c, r)
                GlobalVars.gTileMiPMapX4(idx, 9, r, c) = GlobalVars.gTileMiPMapX1(0, inv, csize - 1 - c, r)
                GlobalVars.gTileMiPMapX4(idx, 10, r, c) = GlobalVars.gTileMiPMapX1(0, 0, r, c)
                GlobalVars.gTileMiPMapX4(idx, 11, r, c) = GlobalVars.gTileMiPMapX1(0, inv, r, csize - 1 - c)
                GlobalVars.gTileMiPMapX4(idx, 12, r, c) = GlobalVars.gTileMiPMapX1(0, inv, c, rsize - 1 - r)
                GlobalVars.gTileMiPMapX4(idx, 13, r, c) = GlobalVars.gTileMiPMapX1(0, 0, csize - 1 - c, rsize - 1 - r)
                GlobalVars.gTileMiPMapX4(idx, 14, r, c) = GlobalVars.gTileMiPMapX1(0, inv, rsize - 1 - r, c)
                GlobalVars.gTileMiPMapX4(idx, 15, r, c) = GlobalVars.gTileMiPMapX1(0, 0, rsize - 1 - r, csize - 1 - c)
            Next
        Next
    End Sub

    '#
    '# Build a X4 scaled MiPMap from the X1 MiPMap with Clockwise rotation and 
    '# Horizontal/Vertical mirror of the resulting X2 MiPMap. Do following two 
    '# stage operation:
    '#
    '#     1. Copy from (r, c) to: (c, max-r), (max-c, r), (max-r, max-c)
    '#     2. Copy from (r, c) to: (r, max-c), (max-r, c), (max-r, max-c)
    '#
    Public Sub BuildMiPMap_RotateClockMirror()
        Dim r As Integer, c As Integer, inv As Integer
        Dim rsize As Integer, csize As Integer, idx As Integer

        rsize = GlobalVars.gRows
        csize = GlobalVars.gCols

        idx = GetIndexFromMap(TileModeTypes.RotateClockMirror)

        If GlobalVars.gInvert = True Then
            inv = 1
        Else
            inv = 0
        End If

        ' Rotate-clockwise involves SWAP + FLIP-COLUMN operation. A point at (r, c) 
        ' moves to (c, max-r). Mirror operation involves in FLIP-COLUMN / FLIP-ROW
        ' operation.
        '
        ' With | A  B |
        '      | C  D | as a result of Rotate-clockwise operation, the new map with Mirror
        ' operation gives,
        '
        '      | A  B  C  A |
        '      | C  D  D  B |
        '      | B  D  D  C |
        '      | A  C  B  A |,  the row/column transform of the elements w.r.t the X1 maps are,
        '
        '       (r, c)       (max-c, r)       (c, r)       (r, max-c)
        '       (c, max-r)   (max-r, max-c)   (max-r, c)   (max-c, max-r)
        '       (c, r)       (r, max-c)       (r, c)       (max-c, r)
        '       (max-r, c)   (max-c, max-r)   (c, max-r)   (max-r, max-c)
        '
        For r = 0 To rsize - 1
            For c = 0 To csize - 1
                GlobalVars.gTileMiPMapX4(idx, 0, r, c) = GlobalVars.gTileMiPMapX1(0, 0, r, c)
                GlobalVars.gTileMiPMapX4(idx, 1, r, c) = GlobalVars.gTileMiPMapX1(0, inv, csize - 1 - c, r)
                GlobalVars.gTileMiPMapX4(idx, 2, r, c) = GlobalVars.gTileMiPMapX1(0, 0, c, r)
                GlobalVars.gTileMiPMapX4(idx, 3, r, c) = GlobalVars.gTileMiPMapX1(0, inv, r, csize - 1 - c)
                GlobalVars.gTileMiPMapX4(idx, 4, r, c) = GlobalVars.gTileMiPMapX1(0, inv, c, rsize - 1 - r)
                GlobalVars.gTileMiPMapX4(idx, 5, r, c) = GlobalVars.gTileMiPMapX1(0, 0, rsize - 1 - r, csize - 1 - c)
                GlobalVars.gTileMiPMapX4(idx, 6, r, c) = GlobalVars.gTileMiPMapX1(0, inv, rsize - 1 - r, c)
                GlobalVars.gTileMiPMapX4(idx, 7, r, c) = GlobalVars.gTileMiPMapX1(0, 0, csize - 1 - c, rsize - 1 - r)
                GlobalVars.gTileMiPMapX4(idx, 8, r, c) = GlobalVars.gTileMiPMapX1(0, 0, c, r)
                GlobalVars.gTileMiPMapX4(idx, 9, r, c) = GlobalVars.gTileMiPMapX1(0, inv, r, csize - 1 - c)
                GlobalVars.gTileMiPMapX4(idx, 10, r, c) = GlobalVars.gTileMiPMapX1(0, 0, r, c)
                GlobalVars.gTileMiPMapX4(idx, 11, r, c) = GlobalVars.gTileMiPMapX1(0, inv, csize - 1 - c, r)
                GlobalVars.gTileMiPMapX4(idx, 12, r, c) = GlobalVars.gTileMiPMapX1(0, inv, rsize - r - 1, c)
                GlobalVars.gTileMiPMapX4(idx, 13, r, c) = GlobalVars.gTileMiPMapX1(0, 0, csize - 1 - c, rsize - 1 - r)
                GlobalVars.gTileMiPMapX4(idx, 14, r, c) = GlobalVars.gTileMiPMapX1(0, inv, c, rsize - 1 - r)
                GlobalVars.gTileMiPMapX4(idx, 15, r, c) = GlobalVars.gTileMiPMapX1(0, 0, rsize - 1 - r, csize - 1 - c)
            Next
        Next
    End Sub

    Private Function PrepareMiPMapForDrawingMode(map As Integer, ByRef MiPMap(,,) As Integer) As Integer
        Dim mask As Integer
        Dim ntables As Integer, rsize As Integer, csize As Integer
        Dim tbl As Integer, row As Integer, col As Integer, idx As Integer

        idx = GetIndexFromMap(map)
        mask = 0

        If GlobalVars.gInvert = False And map = TileModeTypes.Normal Then
            mask = (2 ^ 0) - 1
            rsize = GlobalVars.gRows
            csize = GlobalVars.gCols
            ntables = 1

            ReDim MiPMap(ntables, rsize - 1, csize - 1)
            For tbl = 0 To ntables - 1
                For row = 0 To rsize - 1
                    For col = 0 To csize - 1
                        MiPMap(tbl, row, col) = GlobalVars.gTileMiPMapX1(idx, tbl, row, col)
                    Next
                Next
            Next

        ElseIf map = TileModeTypes.Normal Or _
               map = TileModeTypes.Mirror Or _
               map = TileModeTypes.RotateClock Then

            mask = (2 ^ 1) - 1
            rsize = GlobalVars.gRows
            csize = GlobalVars.gCols
            ntables = 4

            ReDim MiPMap(ntables, rsize - 1, csize - 1)
            For tbl = 0 To ntables - 1
                For row = 0 To rsize - 1
                    For col = 0 To csize - 1
                        MiPMap(tbl, row, col) = GlobalVars.gTileMiPMapX2(idx, tbl, row, col)
                    Next
                Next
            Next

        ElseIf map = TileModeTypes.MirrorRotateClock Or _
               map = TileModeTypes.RotateClockMirror Then

            mask = (2 ^ 2) - 1
            rsize = GlobalVars.gRows
            csize = GlobalVars.gCols
            ntables = 16

            ReDim MiPMap(ntables, rsize - 1, csize - 1)
            For tbl = 0 To ntables - 1
                For row = 0 To rsize - 1
                    For col = 0 To csize - 1
                        MiPMap(tbl, row, col) = GlobalVars.gTileMiPMapX4(idx, tbl, row, col)
                    Next
                Next
            Next
        End If

        PrepareMiPMapForDrawingMode = mask
    End Function

    Private Sub PaintImgCanvasQuad(map As Integer)
        Dim xref As Integer, yref As Integer
        Dim K As Integer, L As Integer, _k As Integer, _l As Integer
        Dim new_x As Integer, new_y As Integer
        Dim dx As Integer, dy As Integer
        Dim xblk As Integer, yblk As Integer
        Dim M As Integer, N As Integer
        Dim row As Integer, col As Integer
        Dim v As Integer, clr As Integer
        Dim mask As Integer, MiPMap(1, 1, 1) As Integer

        M = GlobalVars.gRows
        N = GlobalVars.gCols
        K = (GlobalVars.gBmp.PixelWidth / M)
        L = (GlobalVars.gBmp.PixelHeight / N)
        _k = GlobalVars.gRowspan
        _l = GlobalVars.gColspan

        ' Prepare MipMap
        mask = PrepareMipMapForDrawingMode(map, MiPMap)

        For row = 0 To GlobalVars.gRows - 1
            For col = 0 To GlobalVars.gCols - 1
                For xblk = 0 To K / _k - 1
                    For yblk = 0 To L / _l - 1
                        xref = xblk * (_k * M)
                        yref = yblk * (_l * N)

                        ' Use per-block based color table
                        v = (xblk And mask) + (mask + 1) * (yblk And mask)
                        clr = MiPMap(v, row, col)

                        For dx = 0 To _k - 1
                            For dy = 0 To _l - 1
                                new_x = xref + _k * col + dx
                                new_y = yref + _l * row + dy
                                GlobalVars.gBmp.Pixels(new_x + new_y * (K * M)) = clr
                            Next
                        Next
                    Next
                Next
            Next
        Next

        GlobalVars.gBmp.Invalidate()
        imgCanvas.Source = GlobalVars.gBmp
    End Sub

    Private Sub ParseAndLoadParams()
        Dim str As String
        Dim row As Integer, col As Integer
        Dim clr As Integer, inv_clr As Integer

        str = ""
        If NavigationContext.QueryString.TryGetValue("rowcount", str) = True Then
            GlobalVars.gRows = Convert.ToInt32(str)
        End If
        If NavigationContext.QueryString.TryGetValue("colcount", str) = True Then
            GlobalVars.gCols = Convert.ToInt32(str)
        End If
        If NavigationContext.QueryString.TryGetValue("rowspan", str) = True Then
            GlobalVars.gRowspan = Convert.ToInt32(str)
        End If
        If NavigationContext.QueryString.TryGetValue("colspan", str) = True Then
            GlobalVars.gColspan = Convert.ToInt32(str)
        End If
        If NavigationContext.QueryString.TryGetValue("altinvert", str) = True Then
            GlobalVars.gInvert = True
        Else
            GlobalVars.gInvert = False
        End If

        ' Initialize MiPMaps for all drawing modes
        RedimAllMiPMaps()

        ' Populate color table, by default only X1 MiPMap is loaded
        For row = 0 To GlobalVars.gRows - 1
            If NavigationContext.QueryString.TryGetValue("row" + Convert.ToString(row), str) = True Then
                Dim tagArr() As Char = str.ToCharArray
                For col = 0 To GlobalVars.gCols - 1
                    If tagArr(col) = "1" Then
                        clr = &HFFFFFFFF
                    Else
                        clr = &HFF000000
                    End If
                    inv_clr = clr Xor &HFFFFFF

                    ' Regular map
                    GlobalVars.gTileMiPMapX1(0, 0, row, col) = clr
                    GlobalVars.gTileMiPMapX1(0, 1, row, col) = inv_clr
                Next
            End If
        Next
    End Sub

    Protected Overrides Sub OnNavigatedTo(e As NavigationEventArgs)
        Dim str As String
        Dim map As Integer

        MyBase.OnNavigatedTo(e)

        str = ""

        ' If only a redraw is to be done, do not parse parameters, just repaint
        If NavigationContext.QueryString.TryGetValue("redraw", str) = False Then
            ParseAndLoadParams()
        End If

        ' Build further MiPMaps
        BuildMiPMap_Normal()
        BuildMiPMap_Mirror()
        BuildMiPMap_RotateClock()
        BuildMiPMap_MirrorRotateClock()
        BuildMiPMap_RotateClockMirror()

        ' Draw mode (Mirror, Rotate)
        If NavigationContext.QueryString.TryGetValue("drawmode", str) = True Then
            map = Convert.ToInt32(str)
            If map < TileModeTypes.MaxTypes Then
                PaintImgCanvasQuad(map)
            End If
        End If
    End Sub

    Private Sub imgCanvas_DoubleTap(sender As Object, e As GestureEventArgs) Handles imgCanvas.DoubleTap
        NavigationService.GoBack()
    End Sub
End Class
