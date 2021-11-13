import requests, datetime

# Server Side (TicketingSever.py) Validation Tests
# E => event table; S => seating table, U => user table, T => ticket table


#Test Case E1
# pass empty dict
event1 = {}

#Test Case E2
# Negative price
event2 = {"title": "Invalid", "desription": "Event Description", "priceperseat": -2, "durationhours": 1, "dateandtime": datetime.datetime.now().strftime("%m/%d/%Y, %H:%M:%S")}

#Test Case E3
# 0 duration
event3 = {"title": "Invalid", "desription": "Event Description", "priceperseat": 2, "durationhours": 0, "dateandtime": datetime.datetime.now().strftime("%m/%d/%Y, %H:%M:%S")}

#Test Case E4
# wrong date time format
event2 = {"title": "Invalid", "desription": "Event Description", "priceperseat": 2, "durationhours": 0, "dateandtime": datetime.datetime.now().strftime("%c")}

#Test Case E5
# get non-existing event 

#Test Case E6
# delete non-existing event 

#Test Case S1
# pass empty dict 

#Test Case S2
# single length seat specifications 

#Test Case S3 
# create seats for non-existing event

#Test Case S4
# row and col are flipped 

#Test Case U1
# get non-existing user 

#Test Case U2
# pass empty dict

#Test Case U3
# 

# Test Case T1
# 

r = requests.post("http://localhost:5000/events", json=event1)
print(f'Response Code For First Post: {r.status_code}\n{r.text}')