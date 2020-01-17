// Sample Arduino MAX6675 Arduino Sketch
 
#include "max6675.h"
 
int ktcSO = 8;
int ktcCS = 9;
int ktcCLK = 10;
 
MAX6675 ktc(ktcCLK, ktcCS, ktcSO);
 
   
void setup() {
  Serial.begin(9600);
}
 
void loop() {
  // basic readout test

   float temp = ktc.readCelsius();

   // if temperature is not a number, the skip.
   if(isnan(temp))
    return;
    
   Serial.println(temp);

   // wait 1 seconds and then redo
   delay(1000);
}
