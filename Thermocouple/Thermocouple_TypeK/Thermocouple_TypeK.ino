#include <Wire.h>
#include "RTClib.h"

#include <SoftwareSerial.h>
#include <SparkFunMAX31855k.h> // Using the max31855k driver
#include <SPI.h>  // Included here too due Arduino IDE; Used in above header
#include <SD.h>

// Pins for the thermocouple
const uint8_t CHIP_SELECT_PIN = 10; // Using standard CS line (SS)
const uint8_t VCC = 14; // Powering board straight from Arduino Pro Mini
const uint8_t GND = 15;

// Instantiate an instance of the SparkFunMAX31855k class
SparkFunMAX31855k probe(CHIP_SELECT_PIN, VCC, GND);

// Bluetooth setup
SoftwareSerial BTserial(0,1); // RX | TX
const byte BTpin = 3; // STATE pin to arduino pin D4

// Micro SD card adapter setup
File myFile;

//DateTime
RTC_DS3231 rtc;
char DateAndTimeString[20]; // 19 digits plus the null char

/**
 * setup constructor to initalize the sketch
 */
void setup() {
  Wire.begin(); // for recording the datetime
  
  pinMode(BTpin, INPUT);

  Serial.begin(9600);
  
  rtc.adjust(DateTime(F(__DATE__), F(__TIME__)));

  setupSDCard();
  
  delay(100);  // Let IC stabilize or first readings will be garbage
}
/**
 * Continuous loop every x seconds.
 */
void loop() {
  // Read the temperature in Celsius
  float temperature = probe.readTempC();
    
  if(digitalRead(BTpin) == 1) // the arduino's bluetooth sensor is connected to another device
  {
    Serial.print(temperature);
    Serial.print(" ");
    Serial.print(getCurrentDateTime());
    Serial.println(); 
  }
  else // the arduino's bluetooth sensor is NOT connected to another device
  {
    // config to write to SD card reader here
  }
  
  // Delay 1 second and get next reading
  delay(1000);
}

/**
 * Get the current date and time print in the format
 */
String getCurrentDateTime() {
  DateTime now = rtc.now();
  sprintf_P(DateAndTimeString, PSTR("%02d/%02d/%4d %02u:%02u:%02u"), now.day(), now.month(), now.year(), now.hour(), now.minute(), now.second());
  return DateAndTimeString;
}

/**
 * Setup the sd card for usage
 */
void setupSDCard(){
  if(!SD.begin(4)) {
    Serial.println("Init failed!");
    //while(1);
  }
  Serial.println("Init done.");
}

/**
 * Access a file on the sd card
 */
void accessFileOnSDCard(){
  myFile = SD.open("data.txt", FILE_WRITE);
  if(myFile) // my file opened correctly
  {
    Serial.println("the file was opened correctly");
  }
  else
  {
    Serial.println("ERROR!!!");  
  }
}
