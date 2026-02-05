#include <IRrecv.h>
#include <IRremoteESP8266.h>
#include <IRutils.h>


const byte irPin = D4;  // input pin that the interruption will be attached to

const uint8_t kTimeout = 90;
const uint16_t kCaptureBufferSize = 1024;

IRrecv irrecv(irPin, kCaptureBufferSize, kTimeout, true);
decode_results results;  // Somewhere to store the results

void setup() {
  Serial.begin(115200);
  irrecv.enableIRIn();  // Start the receiver
}

void loop() {
 
   if (irrecv.decode(&results)) {

      String value = resultToHexidecimal(&results);
      Serial.println(value);
      yield(); 
   }

}