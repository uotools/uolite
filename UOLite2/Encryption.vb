﻿'http://kec.cz/tartaros/steamengine/uploads/Keirs%20packet%20guide/www.kairtech.com/uo/info/encryption.htm

'Game play encryption should use TwoFish.

Partial Class LiteClient
    'Seed, CurrentKey0, and CurrentKey1 are generated by UOLite (and the real client) during login.
    Private _CurrentKey0 As UInt32
    Private _CurrentKey1 As UInt32
    Private _EncryptionSeed As UInt32

    'The two keys from the actual client.
    Private _FirstClientKey As UInt32
    Private _SecondClientKey As UInt32

    ''' <summary>Generates two keys for client side encryption.</summary>
    ''' <param name="Seed">The Seed to generate the keys, usualy the client's IP.</param>
    Private Sub GenerateEncryptKeys(ByRef Seed As UInt32)
        _EncryptionSeed = Seed
        _CurrentKey0 = CUInt((((Not _EncryptionSeed) Xor (&H1357)) << 16) Or ((_EncryptionSeed Xor (&HFFFFAAAA)) & (&HFFFF)))
        _CurrentKey1 = CUInt(((_EncryptionSeed Xor (&H43210000)) >> 16) Or (((Not _EncryptionSeed) Xor (&HABCDFFFF)) & (&HFFFF0000)))
    End Sub

    'Ported from C++
    Private Sub LoginCryptBytes(ByRef Bytes() As Byte)
        'int len = data.Length;
        Dim Len As Integer = Bytes.Length

        Dim oldkey0 As UInt32
        Dim oldkey1 As UInt32

        'for(int i = 0; i < len; i++)
        For i As Integer = 0 To Len - 1

            'Decrypt the byte:
            'data[i] = (byte)(CurrentKey0 ^ data[i]);
            Bytes(i) = CByte(_CurrentKey0 ^ Bytes(i))

            'Reset the keys:
            'uint oldkey0 = CurrentKey0;
            oldkey0 = _CurrentKey0

            'uint oldkey1 = CurrentKey1;
            oldkey1 = _CurrentKey1

            'CurrentKey0 = (uint)(((oldkey0 >> 1) | (oldkey1 << 31)) ^ SecondClientKey);
            _CurrentKey0 = CUInt(((oldkey0 >> 1) Or (oldkey1 << 31)) Xor _SecondClientKey)

            'CurrentKey1 = (uint)(((((oldkey1 >> 1) | (oldkey0 << 31)) ^ (FirstClientKey - 1)) >> 1) | (oldkey0 << 31)) ^ FirstClientKey);
            _CurrentKey1 = CUInt((((((oldkey1 >> 1) Or (oldkey0 << 31)) Xor (_FirstClientKey - 1)) >> 1) Or (oldkey0 << 31)) Xor _FirstClientKey)

        Next

    End Sub

    'TODO: One fish, two fish, red fish, blue fish....
    Private Sub GameCryptBytes(ByRef Bytes() As Byte)
        'int len = data.Length;
        Dim Len As Integer = Bytes.Length

        Dim oldkey0 As UInt32
        Dim oldkey1 As UInt32

        'for(int i = 0; i < len; i++)
        For i As Integer = 0 To Len - 1

            'Decrypt the byte:
            'data[i] = (byte)(CurrentKey0 ^ data[i]);
            Bytes(i) = CByte(_CurrentKey0 ^ Bytes(i))

            'Reset the keys:
            'uint oldkey0 = CurrentKey0;
            oldkey0 = _CurrentKey0

            'uint oldkey1 = CurrentKey1;
            oldkey1 = _CurrentKey1

            'CurrentKey0 = (uint)(((oldkey0 >> 1) | (oldkey1 << 31)) ^ SecondClientKey);
            _CurrentKey0 = CUInt(((oldkey0 >> 1) Or (oldkey1 << 31)) ^ _SecondClientKey)

            'CurrentKey1 = (uint)(((((oldkey1 >> 1) | (oldkey0 << 31)) ^ (FirstClientKey - 1)) >> 1) | (oldkey0 << 31)) ^ FirstClientKey);
            _CurrentKey1 = CUInt((((((oldkey1 >> 1) Or (oldkey0 << 31)) ^ (_FirstClientKey - 1)) >> 1) Or (oldkey0 << 31)) ^ _FirstClientKey)

        Next

    End Sub

End Class