#include <Wire.h>
#include "RTClib.h"

#include <SoftwareSerial.h>
#include <SparkFunMAX31855k.h> // Using the max31855k driver
#include <SPI.h>  // Included here too due Arduino IDE; Used in above header
#include <SD.h>
#include <TinyGPS.h>

// Pins for the thermocouple
const uint8_t CHIP_SELECT_PIN = 7; // Using standard CS line (SS)
const uint8_t VCC = 14; // Powering board straight from Arduino Pro Mini
const uint8_t GND = 15;

// Instantiate an instance of the SparkFunMAX31855k class
SparkFunMAX31855k probe(CHIP_SELECT_PIN, VCC, GND);

// Bluetooth setup
SoftwareSerial BTserial(0,1); // RX | TX
const byte BTpin = 6; // STATE pin to arduino pin D4

// Micro SD card adapter setup
File myFile;

//GPS
TinyGPS gps;
SoftwareSerial ss(3, 4);

//DateTime
RTC_DS3231 rtc;
char DateAndTimeString[20]; // 19 digits plus the null char

/**
 * setup constructor to initalize the sketch
 */
void setup() {
  Wire.begin(); // for recording the datetime

  Serial.begin(9600);
  ss.begin(9600);
  
  pinMode(BTpin, INPUT);
  pinMode(10, OUTPUT);

  rtc.adjust(DateTime(F(__DATE__), F(__TIME__)));
  
  delay(100);  // Let IC stabilize or first readings will be garbage
}
/**
 * Continuous loop every x seconds.
 */
void loop() {  
  // Read the temperature in Celsius
  float temperature = probe.readTempC();
  String dateTime = getCurrentDateTime();
  String gpsLocation = getGPSLocation();

  if(isnan(temperature)) // not a number, then can't continue as its wrong
  {
    return;
  }
  

  if(digitalRead(BTpin) >= 1) // the arduino's bluetooth sensor is connected to another device
  {
    Serial.print("-temp ");
    Serial.print(temperature);

    if(dateTime.length() > 0) // has datetime, otherwise dont print
    {
      Serial.print(" -datetime ");
      Serial.print(dateTime);
    }
    
    Serial.print(" ");
    
    Serial.print(gpsLocation);
    Serial.println(); 
  }
  else // the arduino's bluetooth sensor is NOT connected to another device
  {
    writeToFile(temperature, dateTime, gpsLocation);
  }
}

/**
 * Get the current date and time print in the format
 */
String getCurrentDateTime() {
  DateTime now = rtc.now();
  int day = now.day();
  if(day < 1 || day > 31) // invalid day, max month has 31. Usually if RTC is not connected it comes out as 165. 
    return "";
  
  sprintf_P(DateAndTimeString, PSTR("%02d/%02d/%4d %02u:%02u:%02u"), now.day(), now.month(), now.year(), now.hour(), now.minute(), now.second());
  return DateAndTimeString;
}

/**
 * Get the current GPS Location of the sensor
 */
String getGPSLocation(){
  bool newData = false;
  unsigned long chars;
  unsigned short sentences, failed;

  // For one second we parse GPS data and report some key values
  for (unsigned long start = millis(); millis() - start < 1000;)
  {
    while (ss.available())
    {
      //char c = ss.read();
      int i = ss.read();
      
      //Serial.write(c); // uncomment this line if you want to see the GPS data flowing
      if (gps.encode(i)) // Did a new valid sentence come in?
        newData = true;
    }
  }

  if (newData)
  {
    float flat, flon;
    unsigned long age;
    gps.f_get_position(&flat, &flon, &age);

    return String("-lat " + String(flat, 7) + " -long " + String(flon, 7));
  }

  return "";
}

/**
 * Setup the SD Card to write data to
 */
void writeToFile(float temperature, String dateTime, String gpsLocation){
  if(!SD.begin(10)) // can't access sd card so stop
  {
    return;
  }

  myFile = SD.open("data.txt", FILE_WRITE);
  if(myFile)
  {
    myFile.print("-temp ");
    myFile.print(temperature);

    if(dateTime.length() > 0) // has datetime, otherwise dont print
    {
      myFile.print(" -datetime ");
      myFile.print(dateTime);
    }
    
    myFile.print(" ");
    
    myFile.print(gpsLocation);
    myFile.println(); 

    myFile.close();
  }
}
