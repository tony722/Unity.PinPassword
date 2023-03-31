# AET.Unity.PinPassword
### Crestron Module For Handling PIN Security 
It allows changing of PINs too, and stores PINs in .json file on the processor.

To use, download the latest demo:
[AET Unity Pin Password Demo_compiled.zip](https://github.com/tony722/Unity.PinPassword/raw/main/Simpl%20Windows/AET%20Unity%20Pin%20Password%20Demo_compiled.zip)

## Inputs
* **Init:** *Loads config file holding PINs and does other minor setup. Must be run before first use.*

* **Press_Digit[1]-[10]:** *For entering PINs. IMPORTANT: [10] is actually the number [0]. When the number of digits pressed = `Pin_Length` it automatically verifies the PIN (Unless in Change_Pin mode when it's necessary to hit "Save")*
* **Backspace:** *For making corrections when entering PINs.*
* **Clear:** *Clears the pin being entered, cancels out of `Change_Pin` mode.*
* **Change_Pin[1-10]:** *Enters change mode for a particular PIN. Enter the desired digits, and hit Save.*
* **Delete:** *Deletes the PIN being changed (see `Change_Pin[]`).*
* **Save:** *Saves the PIN being changed (see `Change_Pin[]`).*

## Outputs
* **Valid_Pin_Entered:** *Pulses when any valid PIN is entered, including Backdoor,*
* **Invalid_Pin_Entered:** *Pulses when any invalid PIN is entered.*
* **Change_Pin_Success:** *Pulses when a PIN is successfully changed/saved.*
* **Change_Pin_Cancelled:** *Pulses when cancelling out of `Change_Pin` mode without saving.*
* **Backdoor_Pin_Entered:** *Pulses when the backdoor PIN is entered.*
* **Pin_Entered[1]-[10]:** *Pulses when the corresponding PIN is entered.*
* **Change_Pin_F[1]-[10]:** *High when the corresponding PIN is being changed.*
* **Change_Pin_Active:** *High when any PIN is being changed.*
* **Change_Pin_Inactive:** *High when no PINs are being changed.*
* **Pin_Display:** *Displays the PIN as stars you enter it. If `Reveal_Pin` is high, displays actual numbers of PIN.*
* **Message:** *Displays various status messages.*

## Parameters
* **Pin_Length:** *All PINs should be this length exactly.*
* **Backdoor_Pin:** *For dealer or backdoor use.*
* **File_Path:** *Location to store PINS config file. If multiple AET Pin modules are used in one program, each must use a separate config file name.*

## Licensing and Support
Module by Tony Evert and is licensed [Apache License 2.0](https://www.tldrlegal.com/license/apache-license-2-0-apache-2-0).
I'm happy to offer paid support, custom modifications, and Crestron programming to assist you in any way needed. Contact me via http://iconsultants.net

## Source
Full source is available here on GitHub to allow you to make any desired modifications. _If you feel your modification would be of general interest, please clone this repository and issue a pull request. Thanks!_

Referenced libraries:
* [Unity.SimplSharp](https://github.com/tony722/Unity.SimplSharp)