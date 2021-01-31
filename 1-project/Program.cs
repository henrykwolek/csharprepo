using System;
using System.Text;
using System.Linq;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Diagnostics;

namespace C_ {
  //Autor: Henryk Wołek IIC
  class Program {
    //Funkcja która jako argument przyjmuje hasło użytkownika (string), a zwraca wartość hashu SHA256 tej zmiennej
    public static String sha256_hash(String userPassword) {
      StringBuilder Sb = new StringBuilder();

      //Kodowanie utf8 umożliwi wczytanie polskich znaków
      using(SHA256 hash = SHA256Managed.Create()) {
        Encoding enc = Encoding.UTF8;
        //Pierwotny rezultat to tablica typu byte[], jednak jest ona zamieniana na zwykłą zmienną string
        Byte[] result = hash.ComputeHash(enc.GetBytes(userPassword));

        foreach(Byte b in result)
        Sb.Append(b.ToString("x2"));
      }
      //Zwracanie ostatecznej wartości
      return Sb.ToString();
    }
    static void Main(string[] args) {
      //Zebranie danych od użytkownika
      Console.WriteLine("KX crypter by Henryk Wołek\nPodaj słowo do zaszyfrowania!: \n");
      string userMessageToBeEncrypted = Console.ReadLine();
      Console.WriteLine("Twoje słowo to " + userMessageToBeEncrypted + ". Teraz podaj swoje sekretne hasło:\n");
      string userPassPhrase = Console.ReadLine();

      //Inicjalizacja pustej tablicy typu char[]
      char[] tempCharArray = userMessageToBeEncrypted.ToArray();
      //Konwersja na listę
      var charListConv = new List < char > (tempCharArray);

      //Dopóki długość słowa które wprowadził użytkownik nie przekroczy 16, będzie się wykonywała pierwsza pętla while
      while (charListConv.Count < 16) {
        //"Doklejenie" tej samej listy a następnie obrócenie całości
        charListConv.AddRange(charListConv);
        charListConv.Reverse();
        //Poniższe 3 linie kodu zamieniają miejscami pierwszy i ostatni znak nowo powstałej listy
        var tmp = charListConv[0];
        charListConv[0] = charListConv[charListConv.Count - 1];
        charListConv[charListConv.Count - 1] = tmp;
      }
      //Jeśli powstałe słowo ma długość nieparzystą, zostanie skrócone o 1 znak
      if (charListConv.Count % 2 != 0) {
        charListConv.RemoveAt(charListConv.Count - 1);
      }
      for (int s = 0; s < charListConv.Count; s++) {
        //Ta pętla ma za zadanie zamienić znaki stojące na miejscach parzystych z miejsami parzystymi
        var tmpv2 = charListConv[s];
        if (s % 2 == 0) {
          charListConv[s] = charListConv[s + 1];
          charListConv[s + 1] = tmpv2;
        }
      }
      //Jeśli natomiast długość powstałego słowa przekroczy 64, lub użytkownik podał takie słowo, co drugi znak zostanie usunięty aż do skutku
      while (charListConv.Count > 64) {
        charListConv.RemoveAt(charListConv.Count - 2);
      }

      //Konwersja listy na zmienną typu string
      string shrunkUserPhrase = string.Join("", charListConv);
      //Utworzenie specjalnego klucza za pomocą SHA256
      char[] alpha = sha256_hash(userPassPhrase).ToCharArray();
      //Zebranie kodów ASCII poszczególnych znaków
      byte[] asciiBytes = Encoding.ASCII.GetBytes(shrunkUserPhrase);

      List < char > emptyCharArr = new List < char > ();

      for (int i = 0; i < asciiBytes.Length; i++) {
        //Ta pętla przypisze liczbowe kody ASCII do powstałego 64-znakowego klucza SHA256
        //Traktując go jako tablice typu char[]
        int asciiNumerator = asciiBytes[i];
        //Jeśli wartość liczbowa ASCII jest większa niż długość tablicy SHA256, od liczby ASCII zostanie odjęta długość tablicy SHA256
        while (asciiNumerator >= alpha.Length) {
          asciiNumerator = asciiNumerator - alpha.Length;
        }
        //Za każdym powtórzeniem pętli wpisuje ona do listy char[] kolejny znak z tablicy SHA256
        emptyCharArr.Add(alpha[asciiNumerator]);
      }

      //Konwersja z listy na tablicę char[]
      char[] encrypted = emptyCharArr.ToArray();
      Console.WriteLine(reinforce(encrypted, alpha));

      //Linia Console.ReadLine(); uniemożliwi natychmiastowe zamknięcie konsoli po zakończonym programie
      Console.ReadLine();
    }

    //Funkcja któa zamienia miejscami elementy stojące na miejscach parzystych i nieparzystych
    //Jako argument przyjmuje tablicę, która ma zostać pomieszana
    static void flipEvenAndOdd(char[] arrChunk) {
      for (int l = 0; l < arrChunk.Length; l++) {
        //Inicjalizacja tymczasowej zmiennej
        var buffer = arrChunk[l];
        if (l % 2 == 0) {
          //Zamiana z użyciem tymczasowej wartości
          arrChunk[l] = arrChunk[l + 1];
          arrChunk[l + 1] = buffer;
        }
      }
    }

    //Funkcja reinforce(), z ang. oznacza "wzmocnić", ma za zadanie jeszcze bardziej zabezpieczyć słowo użytkownika
    //Jako argumenty przyjmuje tablicę, która powstała w funkcji Main(), jak i tablicę która zawiera znaki klucza SHA256
    static char[] reinforce(char[] alreadyEcnryptedArr, char[] secretArrKey) {
      byte[] asciiTempByte = Encoding.ASCII.GetBytes(alreadyEcnryptedArr);
      List < char > toBeReinforced = new List < char > ();

      for (int h = 0; h < alreadyEcnryptedArr.Length; h++) {
        //Za każdym razem wartość int przyjmuje inną wartość liczbową
        int asciiSecNum = asciiTempByte[h];
        while (asciiSecNum >= secretArrKey.Length) {
          //Gdy jest ona większa niż długość podanej tablicy, zostanie ona zmniejszona o jej długość
          asciiSecNum = asciiSecNum - secretArrKey.Length;
          //Za każdym razem tablica jest obracana
          Array.Reverse(secretArrKey);
        }
        //Jeśłi po podzieleniu wartości liczbowej ASCII przez 2 otrzymamy wartość większą lub równą długości tablicy
        //Zostanie do niej dodany element z indeksem 2 razy większym
        if (asciiSecNum / 2 >= secretArrKey.Length) {
          toBeReinforced.Add(secretArrKey[(asciiSecNum * 2)]);
        }
        //Dodatkowe operacje zwiększające bezpieczeństwo
        toBeReinforced.Add(secretArrKey[asciiSecNum + 1]);
        toBeReinforced.Add(secretArrKey[asciiSecNum]);

        //Jeśli w wyniku powyższych operacji długość hashu przekroczy 64 znaki, pętla zakończy się
        if (toBeReinforced.Count >= 64) {
          break;
        }
      }
      char[] reinforced = toBeReinforced.ToArray();
      //Zapisanie wartości int jako długośc tablicy char[] reinforced
      int len = reinforced.Length;

      //Aby podzielić powstałą tablice na dwie części, należy zbadać czy jej długość jest liczbą parzystą.
      int incrementer = 0;
      if (reinforced.Length % 2 != 0) {
        //Jeśli nie, poniższa wartość int wyniesie 1, co w dalszych liniach kodu ma duże znaczenie
        incrementer = 1;
      }
      //Powstanie dwóch części w wyniku dzielenia na pół tablicy char[] reinforced
      //Zastosowałem tutaj sprawdzanie, czy można "idealnie" podzielić na pół, a jeśli nie to dodajemy 1 do długości "dzielenia"
      char[] firstChunkArr = reinforced.Take((reinforced.Length + incrementer) / 2).ToArray();
      char[] secondChunkArr = reinforced.Skip((reinforced.Length + incrementer) / 2).ToArray();

      //Jeśli powstały dwie tablice i obie z nich mają parzyste długości, to zostanie ona obrócona
      //Następnie elementy stojące na miejscach parzystych zostaną zamienione z miejscami nieparzystymi
      //A na końcu pierwszy element zamieni miejsce z ostatnim
      if (secondChunkArr.Length % 2 == 0 && firstChunkArr.Length % 2 == 0) {
        Array.Reverse(firstChunkArr);
        flipEvenAndOdd(secondChunkArr);
        var firstCharBufOne = secondChunkArr[0];
        secondChunkArr[0] = secondChunkArr[secondChunkArr.Length - 1];
        secondChunkArr[secondChunkArr.Length - 1] = firstCharBufOne;
      }
      //W przeciwnym wypadku, zadzieje się dokładnie to samo co wyżej, tylko że zostanie pominięte "mieszanie"
      //Ponieważ liczba miejsc parzystych i nieparzystych jest różna
      else if (secondChunkArr.Length % 2 == 1 && firstChunkArr.Length % 2 == 1) {
        Array.Reverse(secondChunkArr);
        var firstCharBufTwo = firstChunkArr[0];
        firstChunkArr[0] = firstChunkArr[firstChunkArr.Length - 1];
        firstChunkArr[firstChunkArr.Length - 1] = firstCharBufTwo;
      }

      //Zespolenie dwóch tablic i zwrócenie ostatecznej wartośći do funckji Main()
      char[] combinedReinforcedSecurityArray = firstChunkArr.Concat(secondChunkArr).ToArray();
      return combinedReinforcedSecurityArray;
    }
  }
}