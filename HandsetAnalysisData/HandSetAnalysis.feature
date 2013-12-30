Feature: HandSetAnalysis
        Run the handset analysis app to check the app 
		 
Background:
  Given alteryx running at" http://gallery.alteryx.com/"
  And I am logged in using "deepak.manoharan@accionlabs.com" and "P@ssw0rd"
  And I publish the application "handset performance analysis"
  And I check if the application is "Valid"



Scenario Outline: publish and run Handset Performance Analysis App
When I run the handset performance app with sample data 
Then I see output contains "<result>"
Examples: 
| result             |
| Handset Performance Analysis |

