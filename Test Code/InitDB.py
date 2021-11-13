import requests, datetime, json

'''
------------------
CREATE EVENTS
------------------
'''

event_1 = {"title": "Symphonic Hollywood", "description": "Join the beloved conductor Dr. Moore as he presents some totally non-secular music in a manner that makes you question whether if you're still in a conservative university.", "priceperseat":25,"durationhours":2,"dateandtime": datetime.datetime(2021, 11, 30, 8, 30).strftime("%m/%d/%Y, %H:%M:%S")}

event_2 = {"title": "Mid Summer Night's Dream", "description": "Join us for an all-new production of one of Shakespeare’s most beloved comedies, A Midsummer Night’s Dream. A confused pair of young lovers, an amateur acting troupe and some feuding fairies cross paths in an enchanted forest. Chaos and hilarity ensues for “the course of true love never did run smooth.”", "priceperseat":35,"durationhours":4, "dateandtime": datetime.datetime(2021,12, 1, 15, 30).strftime("%m/%d/%Y, %H:%M:%S")}

event_3 = {"title": "Symphonic Wind Band", "description": "Begin your Christmas season with Carols and Classics featuring the BJU Symphonic Wind Band with the Concert Choir and University Singers. You’ll be thrilled as you celebrate Christ’s incarnation with this program of old and new favorites which includes the premiere of a new work by composition faculty member Seth Custer.", "priceperseat":20.50,"durationhours":1.5,"dateandtime":datetime.datetime(2021,12, 8, 20, 15).strftime("%m/%d/%Y, %H:%M:%S")}

'''
------------------
CREATE SEAT CHARTS
------------------
'''
seatingchart_1 = {"seating_chart": "A1, A2, A3, B1, B2, B3, C1, C2, C3"}

seatingchart_2 = {"seating_chart": "A1, A2, A3, A4, B1, B2, B3, B4, C1, C2, C3, C4, D1, D2, D3, D4"}

seatingchart_3 = {"seating_chart": "A1, A2, A3, A4, A5, B1, B2, B3, B4, B5, C1, C2, C3, C4, C5, D1, D2, D3, D4, D5, E1, E2, E3, E4, E5"}


'''
------------------
CREATE USERS
------------------
'''

user_1 = {"email": "jack@bju.edu", "password": "123456", "creditcard_num": "1234-5678-4321-8765", "creditcard_exp": datetime.datetime(2023, 4, 28).strftime("%m/%d/%Y, %H:%M:%S"), "cvv": 321}
#uuid: a4141e98-193d-4ba9-8620-d878c8e4ec38

user_2 = {"email": "jane@bju.edu", "password": "654321"}
#uuid: c6e65980-8c79-49bb-8e75-08f1358d7437


'''
------------------
CREATE TICKET
------------------
'''

ticket_1 = {"eventid": "2", "seats": "A1,A2", "creditcardnum": "1234-5678-4321-8765"}




#Uncomment code below to insert initial records into a blank db
'''
r1 = requests.post("http://localhost:5000/events",json=event_1)
r2 = requests.post("http://localhost:5000/events",json=event_2)
r3 = requests.post("http://localhost:5000/events",json=event_3)
r4 = requests.post("http://localhost:5000/events/1/seating", json=seatingchart_1)
r5 = requests.post("http://localhost:5000/events/2/seating", json=seatingchart_2)
r6 = requests.post("http://localhost:5000/events/3/seating", json=seatingchart_3)
r7 = requests.post("http://localhost:5000/users", json=user_1)
r8 = requests.post("http://localhost:5000/users", json=user_2)
r9 = requests.post("http://localhost:5000/users/a4141e98-193d-4ba9-8620-d878c8e4ec38/tickets", json=ticket_1)

print(f'Response Code For Post: {r1.status_code}\n{r1.text}')
print(f'Response Code For Post: {r2.status_code}\n{r2.text}')
print(f'Response Code For Post: {r3.status_code}\n{r3.text}')
print(f'Response Code For Post: {r4.status_code}\n{r4.text}')
print(f'Response Code For Post: {r5.status_code}\n{r5.text}')
print(f'Response Code For Post: {r6.status_code}\n{r6.text}')
print(f'Response Code For Post: {r7.status_code}\n{r7.text}')
print(f'Response Code For Post: {r8.status_code}\n{r8.text}')
print(f'Response Code For Post: {r9.status_code}\n{r9.text}')
'''
