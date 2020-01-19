#include <SparkFunMAX31855k.h> // Using the max31855k driver
#include <SPI.h>  // Included here too due Arduino IDE; Used in above header

// Define SPI Arduino pin numbers (Arduino Pro Mini)
const uint8_t CHIP_SELECT_PIN = 10; // Using standard CS line (SS)
// SCK & MISO are defined by Arduiino
const uint8_t VCC = 14; // Powering board straight from Arduino Pro Mini
const uint8_t GND = 15;

// Instantiate an instance of the SparkFunMAX31855k class
SparkFunMAX31855k probe(CHIP_SELECT_PIN, VCC, GND);

void setup() {
  Serial.begin(9600);
  delay(100);  // Let IC stabilize or first readings will be garbage
}

void loop() {
  // Read the temperature in Celsius
  float temperature = probe.readTempC();
  
  if (!isnan(temperature)) 
    Serial.println(temperature);
  
  // Delay 1 seconnd and get next reading
  delay(1000);
}
