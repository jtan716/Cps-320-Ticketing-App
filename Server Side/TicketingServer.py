# To use this, 
# pip3 install requests flask
# Then...
# pip3 install flask_sqlalchemy

#Source Code: Dr. Schaub's service3.py from the class examples

#TODO List (Deliverable 1): 
# Create ticket
# Generate Test Casees with blank db
# Comment header for all methods (parameters, return type)


#TODO List (Future Deliverables): 
# UUID Generation (instead of single int)
# UUID for Users, Tickets
# Better Feedback
# Better Validation 
# Admin operation validation 
# Session ID for user login 

#For User Table...
    #store passwords in a more secure manner
    #notify that only Visa, MasterCard, and Discover and valid credit cards
    #have client code store cvv and expiration date

#For Ticket Table...
    #disable the to_dict_all() method later for security reasons

#For Ticket Table...
    #have client notify user of 10 seat booking cap


from flask import Flask, request, abort, jsonify, Response
from werkzeug.exceptions import HTTPException
from datetime import datetime
import json, time, uuid
from flask_sqlalchemy import SQLAlchemy

app = Flask(__name__)
app.config['SQLALCHEMY_DATABASE_URI'] = 'sqlite:///ticketing_app_table.sqlite3'
db = SQLAlchemy(app)

'''
***************************
BEGIN TABLE 1
'''

# Model class 'events'
# Sql Table for Events
class Event(db.Model):
    __tablename__= 'events'
    id = db.Column(db.Integer, primary_key=True)
    title = db.Column(db.String(40), nullable=False)
    description = db.Column(db.String(160), nullable=False)
    priceperseat = db.Column(db.Float, nullable=False)
    durationhours = db.Column(db.Float, nullable=False)
    dateandtime = db.Column(db.DateTime, nullable=False)

    seatingchart = db.relationship("Seating")

    def to_dict(self):        
        # See https://stackoverflow.com/questions/7102754/jsonify-a-sqlalchemy-result-set-in-flask for a general solution
        return { 'id': self.id, 'title': self.title, 'description': self.description, 'priceperseat': self.priceperseat, 'durationhours': self.durationhours, 'dateandtime': self.dateandtime}

'''
END TABLE 1
***************************
'''




'''
***************************
BEGIN TABLE 2
'''

# Model class 'seating'
# Sql Table for Seats
class Seating(db.Model):
    __tablename__= 'seating'
    eventlinkid = db.Column(db.Integer, db.ForeignKey('events.id'), primary_key=True)
    rowid = db.Column(db.String(1), primary_key=True)
    colid = db.Column(db.Integer, primary_key=True)

    def to_dict(self):
        return {'eventlinkid': self.eventlinkid, 'rowid': self.rowid, 'colid': self.colid}


'''
END TABLE 2
***************************
'''



'''
***************************
BEGIN TABLE 3
'''

# Model class 'users'
# Sql Table for Users
class User(db.Model): 
    __tablename__ = 'users'
    id = db.Column(db.String(36), primary_key=True) #STORE AS UUID
    email = db.Column(db.String(64), nullable=False)
    password = db.Column(db.String(64), nullable=False)
    creditcard_number = db.Column(db.String(20), nullable=True)
    creditcard_expdate = db.Column(db.DateTime, nullable=True)
    creditcard_cvv = db.Column(db.Integer(), nullable=True)

    def to_dict_secure(self):
        return {'id': self.id, 'email': self.email}

    def to_dict_all(self):
        return {'id': self.id, 'email': self.email, 'password': self.password, 'creditcard_number': self.creditcard_number, 'creditcard_expdate': self.creditcard_expdate, 'creditcard_cvv': self.creditcard_cvv}
'''
END TABLE 3
***************************
'''


'''
***************************
BEGIN TABLE 4
'''

# Model class 'tickets'
# Sql Table for Tickets
class Ticket(db.Model): 
    __tablename__ = 'tickets'
    id = db.Column(db.String(36), primary_key=True) #STORE AS UUID
    userlinkid = db.Column(db.Integer(), db.ForeignKey('users.id'), nullable=False)
    eventlinkid = db.Column(db.Integer(), db.ForeignKey('events.id'), nullable=False)
    seats_reserved = db.Column(db.String(60),nullable=False) #CAN ONLY BOOK A MAX OF 10 SEATS
    creditcard_used = db.Column(db.Integer(),nullable=False)
    total_price = db.Column(db.Float(),nullable=False)

    def to_dict(self):
        return {"id":self.id,"userlinkid": self.userlinkid,"eventlinkid":self.eventlinkid,"seats_reserved":self.seats_reserved,"total_price":self.total_price}
'''
END TABLE 4
***************************
'''

# Create tables from model classes
db.create_all()

# ----------------------------------------
# Central error handler. 
# Converts abort() and unhandled application exceptions to JSON responses, and logs them
# ----------------------------------------
@app.errorhandler(Exception)
def handle_error(e):
    
    if isinstance(e, HTTPException):
        return jsonify(error=str(e)), e.code

    # An unhandled exception was thrown ... log the details
    data = request.json if request.json else request.data
    app.logger.error('%s %s\nRequest body: %s\n', request.method, request.full_path, data, exc_info=e)

    return jsonify(error=str(e)), 500

@app.before_request
def start_request():
    app.logger.warning('%s %s - Starting request processing', request.method, request.full_path)

'''
***************************
BEGIN TABLE 1 API CALLS (EVENT)
'''

# Validation method
# @param: event id
# @detail: validates that requested event exists
# @return: event record object
def validate_event(event_id: int): 
    event = Event.query.filter_by(id=event_id).first()
    if event is None:
        abort(404, description=f'No event with id {event_id} exists.')
    return event
        
# HTTP Request
# @return list of events
@app.route("/events", methods=['GET'])
def get_eventlist():
    events = Event.query.all()
    return jsonify([event.to_dict() for event in events])

# HTTP Request
# @param: event id
# @return single event in the db
@app.route("/events/<int:event_id>", methods=['GET'])
def get_eventdetail(event_id: int):
    ev = validate_event(event_id)
    if ev:
        return jsonify(ev.to_dict())

# HTTP Request (ADMIN)
# @param: json {title, description, price per seat, duration in hours, date and time of event}
# @detail: creates requested event record; price and duration must be positive; does NOT check for duplicates
# @detail: uploads event record in db
# @return: returns event record in json{}
@app.route("/events", methods=['POST'])
def create_event():
    event_dict = request.get_json()
    try:
        title_in = event_dict['title']
        description_in = event_dict['description']
        pricequote_in = float(event_dict['priceperseat'])
        durationh_in = float(event_dict['durationhours'])
        dateandtime_in = datetime.strptime(event_dict['dateandtime'],"%m/%d/%Y, %H:%M:%S")
        if pricequote_in < 0:
            abort(400, description=f'Price may not be negative.')
        elif durationh_in <= 0:
            abort(400, description=f'Duration may not be 0 or negative.')
        
    except Exception as e:
        abort(400, description=f'Invalid request: {e}')

    ev = Event(title=title_in, description=description_in, priceperseat=pricequote_in,durationhours=durationh_in,dateandtime=dateandtime_in)
    db.session.add(ev)
    db.session.commit()

    return jsonify(ev.to_dict())

# HTTP Request (ADMIN)
# @param: id of event
# @detail: validates if event exists, then removes record from events table
# @return: confirmation message
@app.route("/events/<int:event_id>", methods=['DELETE'])
def delete_event(event_id:int): 
    ev = validate_event(event_id)
    if ev:
        db.session.delete(ev)
        db.session.commit()
    return f'Sucessfully deleted event with id of {event_id}'

'''
END TABLE 1 API CALLS
***************************
'''


'''
***************************
BEGIN TABLE 2 API CALLS (SEATING)
'''

# Validation method
# @param: seating chart in string format "A1, A2, A3, ..." and id of event 
# @detail: checks for proper formatting and creates list of seat records
# @return: list of seat record objects
def validate_seats(seatingchart:str, event_id:int):
    seatchart_list = [s.strip() for s in seatingchart.split(",")]
    seatingchart_return = []

    for seat in seatchart_list:
        if len(seat) != 2:
            abort(400, description=f'Seat specification must be 2 characters long.')
        elif (seat[0].isalpha() is False or seat[0].islower()):
            abort(400, description=f'Row indexes must be capital letters only.')
        elif (seat[1].isnumeric() is False or int(seat[1]) <= 0):
            abort(400, description=f'Column indexes must be whole numbers and non-zero.')
        
    for seat in seatchart_list:
        seatingchart_return.append(Seating(eventlinkid=event_id,rowid=seat[0],colid=seat[1]))

    return seatingchart_return

# HTTP Request
# @param: id of event
# @return: list of seats for that event in json{}
@app.route("/events/<int:event_id>/seating", methods=['GET'])
def get_eventseating(event_id: int):
    seats = Seating.query.filter_by(eventlinkid=event_id).all()
    return jsonify([seat.to_dict() for seat in seats])


# HTTP Request (ADMIN)
# @param: id of event, dict {"seatingchart": "A1, A2, A3, A4, ..."}
# @detail: uploads requested list of seat records in db if valid
# @return: Confirmation message
@app.route("/events/<int:event_id>/seating", methods=['POST'])
def create_eventseatingchart(event_id: int):
    seating_chart_dict = request.get_json()
    ev = validate_event(event_id)
    try:
        seatchart_in = seating_chart_dict["seating_chart"]
        seatchart_list = validate_seats(seatchart_in,event_id)
        
        for seat in seatchart_list:
            db.session.add(seat)
        
        db.session.commit()

    except Exception as e: 
        abort(400, description=f'Invalid request: {e}')

    return f'Successfully creating seating chart for event {event_id} with seats {seatchart_in}'


'''
END TABLE 2 API CALLS
***************************
'''


'''
***************************
BEGIN TABLE 3 API CALLS
'''

# Validation method
# @param: id of user
# @detail: checks if requested user exists in db
# @return: user record object
def validate_user(user_id: str): 
    user = User.query.filter_by(id=user_id).first()
    if user is None:
        abort(404, description=f'No user with id {user_id} exists')
    return user

# HTTP Request
# @return: list of users in the db
@app.route("/users", methods=['GET'])
def get_allusers():
    users = User.query.all()
    return jsonify([user.to_dict_secure() for user in users])


# HTTP Request
# @param: id of user
# @return: list of users in the db
@app.route("/users/<user_id>", methods=['GET'])
def get_userinfo(user_id: str):
    user_in = validate_user(user_id)
    
    return jsonify(user_in.to_dict_all())

# HTTP Request (ADMIN)
# @param: dict {email, password, creditcard number, credit card expiration date, credit card cvv}
# @detail: creates a User object record and inserts it into the db
# @return: user object record in json{}
@app.route("/users", methods=['POST'])
def create_user():
    user_dict = request.get_json()
    try: 
        id_in = str(uuid.uuid4())
        email_in = user_dict["email"]
        password_in = user_dict["password"]
        creditcard_num_in = None
        creditcard_exp_in = None
        creditcard_cvv_in = None
        try: 
            creditcard_num_in = user_dict["creditcard_num"]
            creditcard_exp_in = datetime.strptime(user_dict["creditcard_exp"],"%m/%d/%Y, %H:%M:%S")
            creditcard_cvv_in = user_dict["creditcard_cvv"]
        except:
            "ignore error"

    except Exception as e:
        abort(400, description=f'Invalid request: {e}')


    user = User(id=id_in,email=email_in,password=password_in,creditcard_number=creditcard_num_in,creditcard_expdate=creditcard_exp_in,creditcard_cvv=creditcard_cvv_in)
    db.session.add(user)
    db.session.commit()

    return jsonify(user.to_dict_all())


# HTTP Request (ADMIN)
# @param: id of user
# @detail: deletes the User record if the user exists
# @return: confirmation of message
@app.route("/users/<user_id>", methods=['DELETE'])
def delete_user(user_id:str):
    user_in = validate_user(user_id)

    if user_in:
        db.session.delete(user_in)
        db.session.commit()
        return f'Successfully deleted user of id {user_id}'

'''
END TABLE 3 API CALLS
***************************
'''


'''
***************************
BEGIN TABLE 4 API CALLS
'''

# TICKET VALIDATION 
def validate_ticket(ticket_id: str):
    ticket = Ticket.query.filter_by(id=ticket_id).first()
    if ticket is None:
        abort(404, description=f'No ticket with id {ticket_id} exists')
    return ticket

# MY HTTP REQUEST
# Get user list of tickets
@app.route("/users/<user_id>/tickets", methods=['GET'])
def get_usertickets(user_id: str):
    user_in = validate_user(user_id)
    tickets = Ticket.query.filter_by(userlinkid=user_id)

    return jsonify([ticket.to_dict() for ticket in tickets])

# MY HTTP REQUEST
# Get user single ticket
@app.route("/users/<user_id>/tickets/<ticket_id>", methods=['GET'])
def get_userticket(user_id: str, ticket_id: str):
    user_in = validate_user(user_id)
    ticket_in = validate_ticket(ticket_id)

    return jsonify(ticket_in.to_dict())

# MY HTTP REQUEST
# Delete user specified ticket 
@app.route("/users/<user_id>/tickets/<ticket_id>", methods=['DELETE'])
def delete_userticket(user_id: str, ticket_id: str):
    user_in = validate_user(user_id)
    ticket_in = validate_ticket(ticket_id)

    if ticket_in: 
        db.sesssion.delete(ticket_in)
        db.session.commit()
        return f'Successfully deleted ticket with id of {ticket_id}'

@app.route("/users/<user_id>/tickets", methods=['POST'])
def create_userticket(user_id: str):
    userticket_dict = request.get_json()
    id_in = str(uuid.uuid4())
    try: 
        eventid_in = userticket_dict["eventid"]
        seatsreq_in = userticket_dict["seats"]
        seatlist_in = validate_seats(seatsreq_in,eventid_in)
        creditcard_in = userticket_dict["creditcardnum"]
        userid_in = validate_user(user_id)
        event_in = validate_event(eventid_in)
        totalprice_in  = calculate_ticketprice(len(seatlist_in),event_in)

        t = Ticket(id=id_in,userlinkid=userid_in.id,eventlinkid=eventid_in,seats_reserved=seatsreq_in,creditcard_used=creditcard_in,total_price=totalprice_in)
        db.session.add(t)
        for seat in seatlist_in:
            "TODO: change status of individual seats here"
        
        db.session.commit()

        return jsonify(t.to_dict())

    except Exception as e:
        abort(400, description=f'Invalid request: {e}')
    
    #return f'Suceessfully reserved ticket with id of {id_in}'

def validate_availableseats():
    abort(500, description=f'Not implemented yet.')

def calculate_ticketprice(numseats: int, event: Event):
    if (numseats <= 0 ):
        abort(400, description=f'Cannot book 0 tickets. Must book at least 1 ticket.')
    priceperseat = event.to_dict()["priceperseat"]

    return numseats * priceperseat

'''
END TABLE 4 API CALLS
***************************
'''


# Startup Server 
if __name__ == '__main__':
    app.run(host="0.0.0.0", debug=True)