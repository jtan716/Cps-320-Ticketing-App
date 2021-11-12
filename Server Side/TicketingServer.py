# To use this, 
# pip3 install requests flask
# Then...
# pip3 install flask_sqlalchemy

#Source Code: Dr. Schaub's service3.py from the class examples

from flask import Flask, request, abort, jsonify, Response
from werkzeug.exceptions import HTTPException
from datetime import datetime
import json, time, uuid
from flask_sqlalchemy import SQLAlchemy

app = Flask(__name__)
app.config['SQLALCHEMY_DATABASE_URI'] = 'sqlite:///events.sqlite3'
db = SQLAlchemy(app)

'''
***************************
BEGIN TABLE 1
'''

# Define model class
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
#TODO store passwords in a more secure manner
#TODO notify that only Visa, MasterCard, and Discover and valid credit cards
#TODO have client code store cvv and expiration date
class User(db.Model): 
    __tablename__ = 'users'
    id = db.Column(db.Integer(), primary_key=True) #STORE AS UUID
    email = db.Column(db.String(64), nullable=False)
    password = db.Column(db.String(64), nullable=False)
    creditcard_number = db.Coulmn(db.String(19), nullable=True)
    creditcard_expdate = db.Column(db.DateTime, nullable=True)
    creditcard_cvv = db.Column(db.Integer(), nullable=True)

    def to_dict_secure(self):
        return {'id': self.id, 'email': self.email}

    #TODO disable this method later for security reasons
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
#TODO Have client-side code LIMIT amount to 10 seats
class Ticket(db.Model): 
    __tablename__ = 'tickets'
    id = db.Column(db.Integer(), primary_key=True) #STORE AS UUID
    userlinkid = db.Column(db.Integer(), db.ForeignKey('users.id'), nullable=False)
    eventlinkid = db.Column(db.Integer(), db.ForeignKey('events.id'), nullable=False)
    seats_reserved = db.Column(db.String(60),nullable=False) #CAN ONLY BOOK A MAX OF 10 SEATS
    total_price = db.Column(db.Float(),nullable=False)

    def to_dict(self):
        return {"id":self.id,"userlinkid": self.userlinkid,"eventlinkid":self.eventlinkid,"seats_reserved":self.seats_reserved,"total_price":self.total_price}
'''
END TABLE 4
***************************
'''


db.create_all() # Create tables from model classes

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

# MY HTTP REQUEST 
@app.route("/events", methods=['GET'])
def get_eventlist():
    events = Event.query.all()
    return jsonify([event.to_dict() for event in events])

# MY HTTP REQUEST 
@app.route("/events/<int:event_id>", methods=['GET'])
def get_eventdetail(event_id: int):
    ev = Event.query.filter_by(id=event_id).first()
    if ev:
        return jsonify(ev.to_dict())
    else:
        abort(404, description=f'No event with id {event_id} exists.')

# ADMIN ONLY operation 
# TODO: either delete this method before publication or figure out way to authenticate posts 
# TODO: better error handling (print out better feedback requesting user to post all fields)
# TODO: check events in the db to make sure there are no duplicates 
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
        elif durationh_in < 0:
            abort(400, description=f'Duration may not be negative.')
        
    except Exception as e:
        abort(400, description=f'Invalid request: {e}')

    ev = Event(title=title_in, description=description_in, priceperseat=pricequote_in,durationhours=durationh_in,dateandtime=dateandtime_in)
    db.session.add(ev)
    db.session.commit()

    return jsonify(ev.to_dict())

# ADMIN ONLY operation 
# TODO: either delete this method before publication or figure out way to authenticate posts 
# TODO: figure out better way to specify event to delete
@app.route("/events/<int:event_id>", methods=['DELETE'])
def delete_event(event_id:int): 
    ev = Event.query.filter_by(id=event_id).first()
    if ev:
        db.session.delete(ev)
        db.session.commit()
        return 'Successfully deleted event: ' + ev.title
    else: 
        abort(404, description=f'No event with id {event_id} exists.')


'''
END TABLE 1 API CALLS
***************************
'''



'''
***************************
BEGIN TABLE 2 API CALLS (SEATING)
'''

# MY HTTP REQUEST 
@app.route("/events/<int:event_id>/seating", methods=['GET'])
def get_eventseating(event_id: int):
    seats = Seating.query.filter_by(eventlinkid=event_id).all()
    return jsonify([seat.to_dict() for seat in seats])

# ADMIN ONLY operation 
# TODO: validate existing seat creation requests
@app.route("/events/<int:event_id>/seating", methods=['POST'])
def create_eventseat(event_id: int):
    seat_dict = request.get_json()
    try:
        eventid_in = event_id
        rowid_in = seat_dict["rowid"]
        colid_in = seat_dict["colid"]

        ev = Event.query.filter_by(id=eventid_in).first()
        if ev is None:
            abort(400, description=f'Seat must be for an existing event.')
        elif (colid_in <= 0):
            abort(400, description=f'Column indexes must be whole numbers and non-zero.')
        elif (rowid_in.isalpha() is False or rowid_in.islower()) : 
            abort(400, description=f'Row indexes must be capital letters only.')

    except Exception as e:
        abort(400, description=f'Invalid request: {e}')

    seat_in = Seating(eventlinkid=eventid_in,rowid=rowid_in,colid=colid_in)

    db.session.add(seat_in)
    db.session.commit()

    return jsonify(seat_in.to_dict())

# ADMIN ONLY operation 
# TODO: validate existing seat creation requests
@app.route("/events/<int:event_id>/seatings", methods=['POST'])
def create_eventseatingchart(event_id: int):
    seating_chart_dict = request.get_json()
    try:
        seatchart_in = seating_chart_dict["seating_chart"]
        seatchart_list = [s.strip() for s in seatchart_in.split(",")]

        ev = Event.query.filter_by(id=event_id).first()
        if ev is None:
            abort(400, description=f'Seat must be for an existing event.')

        for seat in seatchart_list:
            if (seat[0].isalpha() is False or seat[0].islower()):
                abort(400, description=f'Row indexes must be capital letters only.')
            elif (seat[1].isnumeric() is False or int(seat[1]) <= 0):
                abort(400, description=f'Column indexes must be whole numbers and non-zero.')
        
        for seat in seatchart_list:
            seat_in = Seating(eventlinkid=event_id,rowid=seat[0],colid=seat[1])
            db.session.add(seat_in)
        
        db.session.commit()

    except Exception as e: 
        abort(400, description=f'Invalid request: {e}')

    return f'Successfully creating seating chart for event {event_id} with seats {seatchart_in}'


'''
END TABLE 2 API CALLS
***************************
'''



# Startup Server 
if __name__ == '__main__':
    app.run(host="0.0.0.0", debug=True)
