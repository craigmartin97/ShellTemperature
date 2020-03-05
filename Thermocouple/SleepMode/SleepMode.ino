#include <avr/sleep.h>//this AVR library contains the methods that controls the sleep modes
#define interruptPin 2 //Pin we are going to use to wake up the Arduino
#include <IRremote.h>

int receiver = 2;
int led = 8;

IRrecv irrecv(receiver);
decode_results results;

void setup() {
  Serial.begin(9600);//Start Serial Comunication
  pinMode(led, OUTPUT);
  pinMode(interruptPin,INPUT_PULLUP);//Set pin d2 to input using the buildin pullup resistor
  irrecv.enableIRIn();
  digitalWrite(led, LOW);
}

void loop() {
 delay(1000);//wait 5 seconds before going to sleep
 if(irrecv.decode(&results))
 {
  String reading = String(results.value, HEX);
  if(reading == "ffa25d")
  {
    Serial.println("PWR BTN: Go to sleep");
    digitalWrite(led, HIGH);   
    Going_To_Sleep();
  }
  irrecv.resume();
 }
}

void Going_To_Sleep(){
    Serial.println("Going to bed now");
    sleep_enable();//Enabling sleep mode
    attachInterrupt(digitalPinToInterrupt(interruptPin), wakeUp, LOW);//attaching a interrupt to pin d2
    set_sleep_mode(SLEEP_MODE_PWR_DOWN);//Setting the sleep mode, in our case full sleep
    digitalWrite(LED_BUILTIN,LOW);//turning LED off
    delay(1000); //wait a second to allow the led to be turned off before going to sleep
    sleep_cpu();//activating sleep mode
    Serial.println("just woke up!");//next line of code executed after the interrupt 
    digitalWrite(LED_BUILTIN,HIGH);//turning LED on
  }

void wakeUp(){
  digitalWrite(led, LOW);
  Serial.println("Interrrupt Fired");//Print message to serial monitor
  sleep_disable();//Disable sleep mode
  detachInterrupt(digitalPinToInterrupt(interruptPin)); //Removes the interrupt from pin 2;
}
